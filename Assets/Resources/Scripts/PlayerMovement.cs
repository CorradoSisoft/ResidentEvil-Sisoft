using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float runMultiplier = 2f;
    public Camera mainCamera;
    public Animator animator;
    public Transform playerModel;
    public float CameraDistance = 2f;
    public float rotationSpeed = 10f;

    [Header("Camera Collision")]
    public float minCameraDistance = 0.5f;
    public float cameraAdjustSpeed = 5f;
    public LayerMask cameraObstacleMask;

    [Header("Shooting")]
    public float mouseRotationSpeed = 3f;
    public Transform shootOrigin; // Trascina qui l'empty GameObject
    public WeaponAmmo weaponAmmo;

    private Rigidbody rb;
    private float currentCameraDistance;
    private bool isShooting = false;
    private float mouseX = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentCameraDistance = CameraDistance;
    }

    void Update()
    {
        HandleShootingInput();
        RotateModel();
        UpdateAnimator();
        CameraFollow();
    }

    void FixedUpdate()
    {
        MovementPlayer();
    }

    void HandleShootingInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isShooting = true;
            animator.SetBool("isShooting", true); // Imposta SUBITO senza aspettare UpdateAnimator
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isShooting = false;
            animator.SetBool("isShooting", false); // Imposta SUBITO
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (isShooting)
        {
            mouseX += Input.GetAxis("Mouse X") * mouseRotationSpeed; // rimosso Time.deltaTime

            // Sparo con click sinistro
            if (Input.GetMouseButtonDown(0))
                Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponAmmo.Reload();
        }
    }

    void Shoot()
    {
        if (!weaponAmmo.CanShoot())
        {
            Debug.Log("Caricatore vuoto!");
            return;
        }

        weaponAmmo.Shoot();

        Ray ray = new Ray(shootOrigin.position, playerModel.forward);
        RaycastHit hit;

        int playerLayer = LayerMask.GetMask("Player");
        int mask = ~playerLayer;

        if (Physics.Raycast(ray, out hit, 100f, mask))
        {
            Debug.Log($"Colpito: {hit.collider.gameObject.name}");
            Debug.DrawRay(shootOrigin.position, playerModel.forward * hit.distance, Color.red, 2f);
            ZombieController zombie = hit.collider.GetComponentInParent<ZombieController>();
            if (zombie != null) zombie.TakeDamage(1);
        }
        else
        {
            Debug.DrawRay(shootOrigin.position, playerModel.forward * 100f, Color.green, 2f);
        }
    }

    void MovementPlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Se si muove, disattiva shooting
        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;
        if (movement.magnitude > 0)
        {
            isShooting = false;
            animator.SetBool("isShooting", false); // Anche qui immediato
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (isShooting) return; // Fermo mentre spara

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = isRunning ? speed * runMultiplier : speed;

        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
        playerModel.localPosition = Vector3.zero;
    }

    void RotateModel()
    {
        if (isShooting)
        {
            // Ruota il modello con il mouse sull'asse Y
            playerModel.rotation = Quaternion.Euler(0f, mouseX, 0f);
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;

        if (movement.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            // Sincronizza mouseX con la rotazione attuale per evitare scatti quando si rientra in shooting
            mouseX = playerModel.eulerAngles.y;
        }
    }

    void UpdateAnimator()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        animator.SetBool("isShooting", isShooting);
        animator.SetBool("isWalking", movement.magnitude > 0 && !isRunning && !isShooting);
        animator.SetBool("isRunning", isRunning && movement.magnitude > 0 && !isShooting);
    }

    void CameraFollow()
    {
        Vector3 desiredCameraPos = transform.position + Vector3.back * CameraDistance;
        Vector3 directionToCamera = (desiredCameraPos - transform.position).normalized;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToCamera, out hit, CameraDistance, cameraObstacleMask))
        {
            float targetDistance = Mathf.Clamp(hit.distance - 0.2f, minCameraDistance, CameraDistance);
            currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetDistance, cameraAdjustSpeed * Time.deltaTime);
        }
        else
        {
            currentCameraDistance = Mathf.Lerp(currentCameraDistance, CameraDistance, cameraAdjustSpeed * Time.deltaTime);
        }

        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.x = transform.position.x;
        cameraPosition.z = transform.position.z - currentCameraDistance;
        mainCamera.transform.position = cameraPosition;
    }
}