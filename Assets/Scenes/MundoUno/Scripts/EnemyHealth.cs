using UnityEngine;

// Script que gestiona la salud, muerte y score de los enemigos
public class EnemyHealth : MonoBehaviour
{
    [Header("Sistema de Salud")]
    [Tooltip("Salud máxima del enemigo")]
    public int maxHealth = 100; // Vida máxima del enemigo
    
    [Header("Recompensa")]
    public int scoreOnDeath = 50; // Puntos que otorga al morir
    
    [Header("Configuración de Muerte")]
    public float destroyDelay = 2f; // Tiempo antes de destruir el enemigo tras morir
    public string deathTrigger = "Die"; // Nombre del trigger de animación de muerte

    private int currentHealth; // Vida actual
    private bool isDead = false; // Estado de muerte

    void Awake()
    {
        // Inicializa la vida al máximo al instanciar el enemigo
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Aplica daño al enemigo. Si la vida llega a 0, ejecuta Die().
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return; // Si ya está muerto, ignora
        
        currentHealth -= amount; // Resta vida
        currentHealth = Mathf.Max(0, currentHealth); // Evita valores negativos

        if (currentHealth <= 0)
            Die(); // Si la vida es 0 o menos, muere
    }
    
    /// <summary>
    /// Devuelve la vida actual (útil para UI)
    /// </summary>
    public int GetCurrentHealth() => currentHealth;
    
    /// <summary>
    /// Devuelve true si el enemigo ya está muerto
    /// </summary>
    public bool IsDead() => isDead;

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Suma puntos al score global si existe GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreOnDeath);

        // Reproduce animación de muerte si hay Animator y trigger configurado
        Animator a = GetComponentInChildren<Animator>();
        if (a != null && !string.IsNullOrEmpty(deathTrigger))
            a.SetTrigger(deathTrigger);

        // Desactiva todos los colliders para evitar más colisiones
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = false;

        // Desactiva el NavMeshAgent para que deje de moverse
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // Destruye el enemigo tras un pequeño delay (permite ver animación de muerte)
        Destroy(gameObject, destroyDelay);
    }
}
