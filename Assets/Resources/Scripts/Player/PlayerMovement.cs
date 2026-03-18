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

    [Header("First Person Camera")]
    public Camera firstPersonCamera;
    public KeyCode switchCameraKey = KeyCode.V;
    public float fpMouseSensitivity = 2f;
    private bool isFirstPerson = false;
    private float fpYaw = 0f;

    [Header("Shooting")]
    public float mouseRotationSpeed = 3f;
    public Transform shootOrigin; // Trascina qui l'empty GameObject
    public WeaponAmmo weaponAmmo;

    [Header("Footsteps")]
    public AudioSource footstepAudio;
    public AudioClip footstepSound;
    public float stepDelay = 0.5f;
    private float stepTimer = 0f;
    private bool wasMoving = false;

    [Header("Weapon Audio")]
    public AudioSource weaponAudio;
    public AudioClip shootSound;
    public AudioClip emptySound;
    public AudioClip reloadSound;

    [Header("Voice")]
    public AudioSource voiceAudio;
    public AudioClip[] damageSounds;  // frasi quando prende danno
    public AudioClip[] healSounds;    // frasi quando si cura

    private Rigidbody rb;
    private float currentCameraDistance;
    private bool isShooting = false;
    private float mouseX = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentCameraDistance = CameraDistance;

        // Assicurati che la FP camera parta disabilitata
        if (firstPersonCamera != null)
            firstPersonCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        if (PauseMenu.IsPaused || InventoryManager.Instance.IsOpen) return;

        // Switch telecamera
        if (Input.GetKeyDown(switchCameraKey))
            ToggleCamera();

        float horizontal = Input.GetAxisRaw("Horizontal"); // ← Raw
        float vertical = Input.GetAxisRaw("Vertical");     // ← Raw
        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;

        HandleFootsteps();
        HandleShootingInput();
        RotateModel();
        UpdateAnimator();

        if (isFirstPerson)
            CameraFollowFirstPerson();
        else
            CameraFollow();
    }

    void FixedUpdate()
    {
        if (PauseMenu.IsPaused || InventoryManager.Instance.IsOpen) return;
        MovementPlayer();
    }

    void ToggleCamera()
    {
        isFirstPerson = !isFirstPerson;

        mainCamera.gameObject.SetActive(!isFirstPerson);

        if (firstPersonCamera != null)
            firstPersonCamera.gameObject.SetActive(isFirstPerson);

        if (isFirstPerson)
        {
            // Sincronizza lo yaw FP con la rotazione attuale del player model
            fpYaw = playerModel.eulerAngles.y;

            // Blocca e nasconde il cursore in FP
            SetCursorLocked();

            // Mostra crosshair
            if (FPCrosshair.Instance != null)
                FPCrosshair.Instance.SetVisible(true);
        }
        else
        {
            // Ripristina cursore in terza persona
            SetCursorFree();

            // Nascondi crosshair
            if (FPCrosshair.Instance != null)
                FPCrosshair.Instance.SetVisible(false);
        }
    }

    void CameraFollowFirstPerson()
    {
        if (firstPersonCamera == null) return;

        // Leggi input mouse solo orizzontale
        fpYaw += Input.GetAxis("Mouse X") * fpMouseSensitivity;

        // Ruota solo sull'asse Y, mantiene X (20°) e Z impostati nel Transform
        float pitchX = firstPersonCamera.transform.localEulerAngles.x;
        float rollZ  = firstPersonCamera.transform.localEulerAngles.z;
        firstPersonCamera.transform.rotation = Quaternion.Euler(pitchX, fpYaw, rollZ);
    }

    public void OnPlayerRespawn()
    {
        enabled = true;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        animator.SetBool("isDeath", false);
        animator.SetBool("isWalking", false);   // ← resetta anche questi
        animator.SetBool("isShooting", false);  // ← per sicurezza
        isShooting = false;
        SetCursorFree();

        // Se era in FP, torna alla terza persona al respawn
        if (isFirstPerson)
            ToggleCamera();

        // Reset salute per evitare Die() immediato se Load() è lento
        PlayerHealth ph = GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.currentHealth = ph.maxHealth;
            ph.NotifyHealthChanged();
        }
    }

    void HandleShootingInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isShooting = true;
            animator.SetBool("isShooting", true); // Imposta SUBITO senza aspettare UpdateAnimator
            SetCursorLocked();
        }

        if (Input.GetMouseButtonUp(1))
        {
            isShooting = false;
            animator.SetBool("isShooting", false); // Imposta SUBITO
            SetCursorFree();
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
            if (weaponAudio != null && reloadSound != null)
                weaponAudio.PlayOneShot(reloadSound);
        }
    }

    void Shoot()
    {
        if (!weaponAmmo.CanShoot())
        {
            // Pistola scarica
            if (weaponAudio != null && emptySound != null)
                weaponAudio.PlayOneShot(emptySound);
            return;
        }
        weaponAmmo.Shoot();

        // Suono sparo
        if (weaponAudio != null && shootSound != null)
            weaponAudio.PlayOneShot(shootSound);

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
                
                // Registra la distruzione prima di distruggere
                SaveableObject saveable = h.collider.GetComponent<SaveableObject>();
                if (saveable != null)
                    SaveManager.Instance.RegisterDestroyed(saveable.uniqueID);
                
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

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDir.magnitude > 0)
        {
            isShooting = false;
            animator.SetBool("isShooting", false);
            SetCursorFree();
        }

        if (isShooting) return;

        Vector3 movement;

        if (isFirstPerson)
        {
            // In FP: ruota l'input in base allo yaw della camera
            Quaternion camYawRotation = Quaternion.Euler(0f, fpYaw, 0f);
            movement = camYawRotation * inputDir;

            // Ruota anche il playerModel nella direzione del movimento
            if (inputDir.magnitude > 0)
                playerModel.rotation = Quaternion.Euler(0f, fpYaw, 0f);
        }
        else
        {
            movement = inputDir;
        }

        bool isRunning = false;
        float currentSpeed = isRunning ? speed * runMultiplier : speed;

        Vector3 targetVelocity = movement * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y; // mantieni la gravità
        rb.linearVelocity = targetVelocity;

        playerModel.localPosition = Vector3.zero;
    }

    public void OnPlayerDeath()
    {
        // Blocca input
        enabled = false;

        // Blocca movimento
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Animazione morte
        animator.SetBool("isDeath", true);
        animator.SetBool("isWalking", false);
        animator.SetBool("isShooting", false);

        // Cursore normale
        SetCursorFree();
    }


    void HandleFootsteps()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isMoving = (h != 0 || v != 0) && !isShooting;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                if (footstepAudio != null && footstepSound != null)
                {
                    footstepAudio.pitch = Random.Range(0.9f, 1.1f);
                    footstepAudio.PlayOneShot(footstepSound);
                }
                stepTimer = stepDelay;
            }
        }
        else
        {
            stepTimer = stepDelay; // reset immediato
        }

        wasMoving = isMoving;
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
        bool isRunning = false ; //Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        animator.SetBool("isShooting", isShooting);
        animator.SetBool("isWalking", movement.magnitude > 0 && !isRunning && !isShooting);
        /* animator.SetBool("isRunning", isRunning && movement.magnitude > 0 && !isShooting); */
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

    /// <summary>
    /// Imposta il cursore solo se NON siamo in modalità FP.
    /// In FP il cursore resta sempre bloccato e invisibile.
    /// </summary>
    void SetCursorFree()
    {
        if (isFirstPerson) return;
        SetCursorFree();
    }

    void SetCursorLocked()
    {
        SetCursorLocked();
    }
}