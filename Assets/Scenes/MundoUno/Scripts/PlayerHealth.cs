using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class PlayerHealthMain : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("UI")]
    public Slider healthBar; // asignar en el Inspector (opcional)

    [Header("Death")]
    public float deathDelay = 0.2f; // tiempo antes de disparar la secuencia (si quieres animaciones)

    bool isDead = false;

    void Start()
    {
        health = Mathf.Clamp(health, 0f, maxHealth);
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = maxHealth;
            UpdateHealthUI();
        }
        else
        {
            Debug.LogWarning("PlayerHealth: 'healthBar' no asignado en el Inspector");
        }
    }

    /// <summary>
    /// Aplica daño al jugador. Si llega a 0 activa la secuencia de muerte.
    /// </summary>
    /// <param name="amount">Cantidad de daño a aplicar (positivo).</param>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health = Mathf.Clamp(health - Mathf.Abs(amount), 0f, maxHealth);
        Debug.Log($"Player Health: {health}/{maxHealth}");

        UpdateHealthUI();

        if (health <= 0f)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthBar == null) return;
        healthBar.value = Mathf.Clamp(health, 0f, maxHealth);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"PLAYER DEAD en escena: {SceneManager.GetActiveScene().name}");

        // Intentar desactivar scripts de control de forma segura (por nombre o por tipo)
        DisableScriptIfExists("FPSPlayerController");
        DisableScriptIfExists("PlayerMovement");
        DisableScriptIfExists("PlayerShooting");

        // También desactivar cualquier MonoBehaviour que tenga nombres parecidos
        var monos = GetComponents<MonoBehaviour>();
        foreach (var m in monos)
        {
            if (m == null) continue;
            string tname = m.GetType().Name.ToLower();
            if (tname.Contains("player") && (tname.Contains("move") || tname.Contains("shoot") || tname.Contains("controller")))
            {
                try { m.enabled = false; } catch { }
            }
        }

        // Desactivar collider para evitar interacciones
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Llamar a LevelManager para la secuencia de Game Over -> volver al primer mundo con conteo (10s)
        if (deathDelay > 0f)
            Invoke(nameof(TriggerReturnSequence), deathDelay);
        else
            TriggerReturnSequence();
    }

    /// <summary>
    /// Intenta desactivar un componente por su nombre. No provoca error de compilación si el tipo no existe.
    /// </summary>
    void DisableScriptIfExists(string scriptTypeName)
    {
        // Intento directo por nombre (GetComponent(string))
        Component compByName = null;
        try
        {
            compByName = GetComponent(scriptTypeName);
        }
        catch { compByName = null; }

        var behaviour = compByName as Behaviour;
        if (behaviour != null)
        {
            behaviour.enabled = false;
            return;
        }

        // Si no fue encontrado, buscamos entre los MonoBehaviour por coincidencia de nombre
        var monos = GetComponents<MonoBehaviour>();
        foreach (var m in monos)
        {
            if (m == null) continue;
            var t = m.GetType();
            if (t.Name == scriptTypeName || (t.FullName != null && t.FullName.EndsWith("." + scriptTypeName)))
            {
                try { m.enabled = false; } catch { }
                return;
            }
        }
    }

    void TriggerReturnSequence()
    {
        Debug.Log($"PlayerHealth.TriggerReturnSequence() llamado en escena: {SceneManager.GetActiveScene().name}");
        
        if (LevelManager.instance != null)
        {
            Debug.Log("LevelManager.instance encontrado - usando TriggerPlayerDeathReturnToFirstWorld");
            // Usar LevelManager para manejar la secuencia completa (con countdown y reset)
            LevelManager.instance.TriggerPlayerDeathReturnToFirstWorld(10f);
        }
        else
        {
            // Fallback: Si no hay LevelManager, iniciar coroutine local
            Debug.LogWarning("PlayerHealth: LevelManager.instance NO encontrada, usando fallback para volver al nivel 1.");
            StartCoroutine(FallbackReturnToFirstWorldCoroutine(10f));
        }
    }

    /// <summary>
    /// Coroutine de respaldo que siempre devuelve al nivel 1 (primerMundo) cuando el jugador muere.
    /// Se usa solo si no hay LevelManager disponible.
    /// </summary>
    private System.Collections.IEnumerator FallbackReturnToFirstWorldCoroutine(float seconds)
    {
        Debug.Log($"[FALLBACK] Iniciando FallbackReturnToFirstWorldCoroutine desde escena: {SceneManager.GetActiveScene().name}");
        
        Time.timeScale = 0f;
        Debug.Log("[FALLBACK] Time.timeScale = 0 (juego pausado)");
        
        float t = seconds;
        
        while (t > 0f)
        {
            Debug.Log($"[FALLBACK] Game Over - Reiniciando en {Mathf.CeilToInt(t)} segundos...");
            yield return new WaitForSecondsRealtime(1f);
            t -= 1f;
        }

        // Reset del GameManager
        if (GameManager.Instance != null)
        {
            Debug.Log("[FALLBACK] Reseteando GameManager.ResetScores()");
            GameManager.Instance.ResetScores();
        }

        Time.timeScale = 1f;
        Debug.Log("[FALLBACK] Time.timeScale = 1 (juego reanudado)");
        
        // SIEMPRE volver a la escena 0 (primerMundo)
        Debug.Log($"[FALLBACK] >>> CARGANDO ESCENA 0 (primerMundo) desde: {SceneManager.GetActiveScene().name} <<<");
        SceneManager.LoadScene(0);
    }

    // Métodos públicos para que otros scripts lean estado sin acceder a fields directamente
    public bool IsDead()
    {
        return isDead;
    }

    public int GetHealth()
    {
        return Mathf.RoundToInt(health);
    }

    public int GetMaxHealth()
    {
        return Mathf.RoundToInt(maxHealth);
    }
}
