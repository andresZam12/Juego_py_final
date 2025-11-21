using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// WarrokEnemy (corregido):
/// - Maneja detección, persecución, ataque y muerte.
/// - Protege llamadas al Animator comprobando que exista el parámetro (evita warnings).
/// - Suma score al GameManager cuando muere (campo scoreOnDeath).
/// - Desactiva IA, agente y colisiones al morir.
/// - Fácil de configurar desde el Inspector.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class WarrokEnemy : MonoBehaviour
{
    [Header("OBJETIVO")]
    public Transform objetivo;

    [Header("VELOCIDAD")]
    public float moveSpeed = 3f;

    [Header("IA")]
    public NavMeshAgent navMeshAgent;
    public float detectionRange = 10f;
    public float rotationSpeed = 5f;
    public bool smoothRotation = true;

    [Header("SONIDOS")]
    public AudioClip ataqueSound;
    public AudioSource audioSourceAtaque;
    public AudioClip muerteSound;
    public AudioSource audioSourceMuerte;

    [Header("HEALTH")]
    public int maxHealth = 100;
    [SerializeField]
    private int health;

    [Header("COMBATE")]
    public int attackDamage = 10;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public int scoreOnDeath = 50;

    [Header("ANIMATION")]
    public Animator animator;
    public string paramIsIdle = "IsIdle";
    public string paramIsRunning = "IsRunning";
    public string paramIsAttacking = "IsAttacking";
    public string paramIsDead = "IsDead";
    public string triggerAttack = "Attack";
    public string triggerDie = "Die";
    
    // Cache de parámetros del animator para evitar búsquedas repetidas
    private bool hasIdleParam, hasRunningParam, hasAttackingParam, hasDeadParam, hasAttackTrigger, hasDieTrigger;
    private bool animatorParamsCached = false;

    private enum EnemyState { Idle, Run, Attack, Death, Exit }
    private EnemyState currentState = EnemyState.Idle;

    private bool isPlayerDetected = false;
    private float lastAttackTime = 0f;
    private Vector3 initialPosition;
    private bool isDead = false;

    void Awake()
    {
        // Componentes por defecto si no son asignados en Inspector
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        // Inicializa salud
        health = maxHealth;

        // Si no se asignó objetivo, buscar por tag "Player"
        if (objetivo == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                objetivo = playerObj.transform;
        }

        // Si hay AudioSources se asignan por orden (opcional)
        SetupAudioSources();
    }

    void Start()
    {
        initialPosition = transform.position;

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance = Mathf.Max(0.1f, attackRange - 0.5f);
            navMeshAgent.updateRotation = false; // manejamos rotación manual para suavizado
        }
        
        // Cachear parámetros del animator una sola vez
        CacheAnimatorParams();
    }

    void SetupAudioSources()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length >= 2)
        {
            if (audioSourceAtaque == null) audioSourceAtaque = audioSources[0];
            if (audioSourceMuerte == null) audioSourceMuerte = audioSources[1];
        }
        else if (audioSources.Length == 1)
        {
            if (audioSourceAtaque == null) audioSourceAtaque = audioSources[0];
        }
    }

    void Update()
    {
        if (isDead || currentState == EnemyState.Death || currentState == EnemyState.Exit)
            return;

        CheckPlayerDetection();
        StateMachine();
        UpdateAnimator();
    }

    void CheckPlayerDetection()
    {
        if (objetivo == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, objetivo.position);
        isPlayerDetected = distanceToPlayer <= detectionRange;

        if (isPlayerDetected && smoothRotation)
        {
            Vector3 direction = (objetivo.position - transform.position).normalized;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void StateMachine()
    {
        if (objetivo == null) return;
        float distanceToPlayer = Vector3.Distance(transform.position, objetivo.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (isPlayerDetected)
                {
                    if (distanceToPlayer <= attackRange)
                        ChangeState(EnemyState.Attack);
                    else
                        ChangeState(EnemyState.Run);
                }
                break;

            case EnemyState.Run:
                if (!isPlayerDetected)
                {
                    ChangeState(EnemyState.Idle);
                    StopNavMeshAgent();
                }
                else if (distanceToPlayer <= attackRange)
                {
                    ChangeState(EnemyState.Attack);
                    StopNavMeshAgent();
                }
                else
                {
                    MoveTowardsPlayer();
                }
                break;

            case EnemyState.Attack:
                if (!isPlayerDetected)
                {
                    ChangeState(EnemyState.Idle);
                }
                else if (distanceToPlayer > attackRange)
                {
                    ChangeState(EnemyState.Run);
                }
                else
                {
                    if (Time.time >= lastAttackTime + attackCooldown)
                    {
                        PerformAttack();
                        lastAttackTime = Time.time;
                    }
                }
                break;
        }
    }

    void MoveTowardsPlayer()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(objetivo.position);
        }
        else
        {
            Vector3 direction = (objetivo.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    void StopNavMeshAgent()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            navMeshAgent.isStopped = true;
    }

    void ChangeState(EnemyState newState)
    {
        currentState = newState;
        if (newState == EnemyState.Death)
            OnDeath();
    }

    // ----------------------
    // Animator helpers (optimizados con cache)
    // ----------------------
    void CacheAnimatorParams()
    {
        if (animator == null || animatorParamsCached) return;
        
        hasIdleParam = HasParamDirect(paramIsIdle);
        hasRunningParam = HasParamDirect(paramIsRunning);
        hasAttackingParam = HasParamDirect(paramIsAttacking);
        hasDeadParam = HasParamDirect(paramIsDead);
        hasAttackTrigger = HasParamDirect(triggerAttack);
        hasDieTrigger = HasParamDirect(triggerDie);
        
        animatorParamsCached = true;
    }
    
    bool HasParamDirect(string name)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return false;
        var pars = animator.parameters;
        for (int i = 0; i < pars.Length; i++)
            if (pars[i].name == name) return true;
        return false;
    }
    
    void UpdateAnimator()
    {
        if (animator == null) return;

        if (hasIdleParam) animator.SetBool(paramIsIdle, currentState == EnemyState.Idle);
        if (hasRunningParam) animator.SetBool(paramIsRunning, currentState == EnemyState.Run);
        if (hasAttackingParam) animator.SetBool(paramIsAttacking, currentState == EnemyState.Attack);
        if (hasDeadParam) animator.SetBool(paramIsDead, currentState == EnemyState.Death);
    }

    void SafeSetTrigger(string name)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return;
        
        // Usar cache para triggers
        bool canSet = false;
        if (name == triggerAttack) canSet = hasAttackTrigger;
        else if (name == triggerDie) canSet = hasDieTrigger;
        
        if (canSet)
        {
            try { animator.SetTrigger(name); } catch { }
        }
    }

    // ----------------------
    // Ataque, sonidos y daño
    // ----------------------
    void PerformAttack()
    {
        PlayAttackSound();
        SafeSetTrigger(triggerAttack);

        if (objetivo != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, objetivo.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth playerHealth = objetivo.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    void PlayAttackSound()
    {
        if (audioSourceAtaque != null && ataqueSound != null)
        {
            audioSourceAtaque.PlayOneShot(ataqueSound);
        }
    }

    void PlayDeathSound()
    {
        if (audioSourceMuerte != null && muerteSound != null)
        {
            audioSourceMuerte.PlayOneShot(muerteSound);
        }
    }

    // ----------------------
    // Salud & Muerte
    // ----------------------
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        ChangeState(EnemyState.Death);
    }

    void OnDeath()
    {
        PlayDeathSound();
        StopNavMeshAgent();

        // deshabilitar scripts de comportamiento si existen
        var ai = GetComponent<EnemyChaseAvoid>();
        if (ai != null) ai.enabled = false;
        var attack = GetComponent<EnemyAttackSimple>();
        if (attack != null) attack.enabled = false;

        // desactivar navmesh
        if (navMeshAgent != null) navMeshAgent.enabled = false;

        // desactivar colisionadores
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = false;

        // sumar score
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreOnDeath);

        // animacion muerte (si existe) y destruccion
        SafeSetTrigger(triggerDie);
        StartCoroutine(DestroyAfterDeathCoroutine());
    }

    IEnumerator DestroyAfterDeathCoroutine()
    {
        // si hay animacion, esperar un poco; sino se destruye rapido
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    // ----------------------
    // Estado publico para LevelManager
    // ----------------------
    public bool IsDead() => isDead;
    public bool IsAlive() => !isDead && health > 0;
    public int GetHealth() => health;
    public int GetMaxHealth() => maxHealth;

    // ----------------------
    // Invizual debug
    // ----------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
