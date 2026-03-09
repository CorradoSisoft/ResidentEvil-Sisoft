using UnityEngine;
using System.Collections;

public class ZombieController : MonoBehaviour
{
    [Header("References")]
    public Transform zombieModel;
    public Animator animator;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float fieldOfView = 90f;
    public LayerMask obstacleMask;

    [Header("Movement")]
    public float walkSpeed = 0.8f;
    public float rotationSpeed = 2f;

    [Header("Health")]
    public int health = 5;

    [Header("Death Effects")]
    public Material bloodMaterial;
    public Transform bloodOrigin; // Trascina qui l'Empty GameObject figlio

    [Header("Attack")]
    public int attackDamage = 1;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;

    private float lastAttackTime = 0f;

    private Rigidbody rb;
    private Transform player;
    private bool isDead = false;
    private bool hasSeenPlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("Player non trovato!");
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (CanSeePlayer())
            hasSeenPlayer = true;

        UpdateAnimator();
        RotateModel();
    }

    void FixedUpdate()
    {
        if (isDead || player == null) return;

        if (hasSeenPlayer)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
                TryAttack();
            else
                ChasePlayer();
        }
    }

    void TryAttack()
    {
        // Fermo quando attacca
        rb.linearVelocity = Vector3.zero;

        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(attackDamage);

        Debug.Log($"Zombie attacca! Danno: {attackDamage}");
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > detectionRange) return false;

        float angle = Vector3.Angle(zombieModel.forward, directionToPlayer);
        if (angle > fieldOfView / 2f) return false;

        // Controlla se c'è un muro in mezzo
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, distance, obstacleMask))
            return false;

        return true;
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;
        
        Debug.Log($"Direction: {direction}, Distance: {Vector3.Distance(transform.position, player.position)}");
        
        rb.MovePosition(rb.position + direction * walkSpeed * Time.fixedDeltaTime);
        zombieModel.localPosition = Vector3.zero;
    }

    void RotateModel()
    {
        if (!hasSeenPlayer) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            zombieModel.rotation = Quaternion.Slerp(zombieModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void UpdateAnimator()
    {
        animator.SetBool("isWalking", hasSeenPlayer);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        health -= damage;
        if (health <= 0)
            Die();
    }

    public void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        animator.SetBool("isWalking", false);
        animator.SetBool("isDeath", true);

        StartCoroutine(SpawnBloodDelayed());
    }

    IEnumerator SpawnBloodDelayed()
    {
        // Aspetta che l'animazione di morte finisca
        yield return new WaitForSeconds(1.5f); // Aggiusta in base alla durata della tua animazione

        Vector3 spawnPos = new Vector3(bloodOrigin.position.x, 0.01f, bloodOrigin.position.z);
        GameObject pool = BloodPool.Create(spawnPos, bloodMaterial);
        pool.transform.SetParent(transform);
        pool.transform.position = spawnPos;
        pool.transform.localRotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
    }
}