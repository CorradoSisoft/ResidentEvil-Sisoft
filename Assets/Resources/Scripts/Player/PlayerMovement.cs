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
        // Blocca tutto se pausa o inventario aperti
        if (PauseMenu.IsPaused || InventoryManager.Instance.IsOpen) return;

        HandleShootingInput();
        RotateModel();
        UpdateAnimator();
        CameraFollow();
    }

    void FixedUpdate()
    {
        if (PauseMenu.IsPaused || InventoryManager.Instance.IsOpen) return;
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
        if (!weaponAmmo.CanShoot()) return;
        weaponAmmo.Shoot();

        Vector3 torsoPosition = transform.position + Vector3.up * 1f;

        int playerLayer = LayerMask.NameToLayer("Player");
        int mask = ~(1 << playerLayer);

        // Controlla se la pistola è oltre un ostacolo
        Vector3 dirToGun = (shootOrigin.position - torsoPosition).normalized;
        float distToGun = Vector3.Distance(torsoPosition, shootOrigin.position);

        RaycastHit wallCheck;
        if (Physics.Raycast(torsoPosition, dirToGun, out wallCheck, distToGun, mask))
        {
            Debug.Log($"Pistola oltre ostacolo: {wallCheck.collider.gameObject.name}, sparo bloccato");
            Debug.DrawRay(torsoPosition, dirToGun * distToGun, Color.yellow, 2f); // giallo = bloccato
            return;
        }

        Debug.DrawRay(torsoPosition, dirToGun * distToGun, Color.cyan, 2f); // ciano = pistola ok

        // Sparo normale
        Ray ray = new Ray(shootOrigin.position, playerModel.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.gameObject.layer == playerLayer) continue;

            // Controlla se è una Barra
            if (h.collider.CompareTag("Barra"))
            {
                Debug.DrawRay(shootOrigin.position, playerModel.forward * h.distance, Color.yellow, 2f);
                Destroy(h.collider.gameObject);
                break;
            }

            ZombieController zombie = h.collider.GetComponentInParent<ZombieController>();
            if (zombie != null)
            {
                Debug.DrawRay(shootOrigin.position, playerModel.forward * h.distance, Color.red, 2f);
                zombie.TakeDamage(1);
            }
            else
            {
                Debug.DrawRay(shootOrigin.position, playerModel.forward * h.distance, Color.green, 2f);
            }
            break;
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