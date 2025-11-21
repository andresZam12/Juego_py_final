using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Sistema de Salud")]
    [Tooltip("Salud máxima del enemigo")]
    public int maxHealth = 100;
    
    [Header("Recompensa")]
    public int scoreOnDeath = 50;
    
    [Header("Configuración de Muerte")]
    public float destroyDelay = 2f;
    public string deathTrigger = "Die";

    private int currentHealth;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Sistema unificado de daño - usar este método para aplicar daño
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
            Die();
    }
    
    /// <summary>
    /// Obtener salud actual (útil para barras de vida)
    /// </summary>
    public int GetCurrentHealth() => currentHealth;
    
    /// <summary>
    /// Verificar si el enemigo está muerto
    /// </summary>
    public bool IsDead() => isDead;

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // sumar puntos (si existe GameManager)
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreOnDeath);

        // intentar reproducir animación de muerte si existe
        Animator a = GetComponentInChildren<Animator>();
        if (a != null && !string.IsNullOrEmpty(deathTrigger))
            a.SetTrigger(deathTrigger);

        // desactivar colisiones y lógica de movimiento inmediatamente
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = false;

        var chase = GetComponent<EnemyChase>();
        if (chase != null) chase.NotifyDead();

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // destruir el gameobject tras un pequeño delay (por la animación, si hay)
        Destroy(gameObject, destroyDelay);
    }
}
