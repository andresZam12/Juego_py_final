using UnityEngine;

public class SkeletonEnemy : MonoBehaviour
{
    [Header("Vida")]
    public float health = 100f;
    
    [Header("Movimiento")]
    public float moveSpeed = 3.5f;
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float rotationSpeed = 5f;
    
    [Header("Ataque")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    
    private Transform player;
    private float lastAttackTime;
    private bool isDead = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead || player == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
            else
            {
                ChasePlayer();
            }
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Rotar hacia el jugador
        direction.y = 0;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            
            VidaPlayer playerHealth = player.GetComponent<VidaPlayer>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
            return;
            
        health -= amount;

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;
            
        isDead = true;

        if (NivelManager.instance != null)
        {
            NivelManager.instance.EnemyKilled();
        }

        Destroy(gameObject);
    }
}