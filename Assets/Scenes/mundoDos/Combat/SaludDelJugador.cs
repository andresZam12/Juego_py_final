using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaludDelJugador : MonoBehaviour
{
    [Header("Vida del jugador")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Slider healthBar;        // Arrastra aquí PlayerHealthBar (Slider)
    public GameObject gameOverPanel; // Arrastra aquí GameOverPanel

    void Start()
    {
        currentHealth = maxHealth;

        // Asignar UI de forma segura
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }
        else
        {
            Debug.LogWarning("PlayerHealth: healthBar NO está asignado en el Inspector.");
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // Ocultar al inicio
        }
        else
        {
            Debug.LogWarning("PlayerHealth: gameOverPanel NO está asignado en el Inspector.");
        }
    }

    // Función llamada por los enemigos cuando te disparan
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (healthBar != null)
            healthBar.value = currentHealth;

        Debug.Log($"[SaludDelJugador] Jugador recibe {damage} de daño. Vida actual: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("[SaludDelJugador] Vida <= 0, llamando a Die()");
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"El jugador ha muerto en escena: {SceneManager.GetActiveScene().name}");

        // Mostrar GAME OVER panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Bloquear controles del jugador aquí si tienes script de movimiento
        // GetComponent<PlayerMovement>().enabled = false;

        // Pausar el juego
        Time.timeScale = 0f;

        // Volver al primer nivel después de 3 segundos (usando tiempo real)
        StartCoroutine(ReturnToFirstLevelCoroutine(3f));
    }

    /// <summary>
    /// Coroutine que espera X segundos y luego vuelve SIEMPRE al primer nivel (escena 0 - primerMundo).
    /// Usa tiempo real porque el juego está pausado.
    /// </summary>
    System.Collections.IEnumerator ReturnToFirstLevelCoroutine(float seconds)
    {
        Debug.Log($"[SaludDelJugador] Esperando {seconds} segundos antes de volver al primer nivel...");
        
        float t = seconds;
        while (t > 0f)
        {
            Debug.Log($"[SaludDelJugador] Reiniciando en {Mathf.CeilToInt(t)} segundos...");
            // Usar WaitForSecondsRealtime porque el juego está pausado (Time.timeScale = 0)
            yield return new WaitForSecondsRealtime(1f);
            t -= 1f;
        }

        // Resetear GameManager si existe
        if (GameManager.Instance != null)
        {
            Debug.Log("[SaludDelJugador] Reseteando scores del GameManager");
            GameManager.Instance.ResetScores();
        }
        else
        {
            Debug.LogWarning("[SaludDelJugador] GameManager.Instance no encontrado");
        }

        // Reanudar el tiempo antes de cambiar de escena
        Time.timeScale = 1f;
        Debug.Log("[SaludDelJugador] Time.timeScale = 1 (juego reanudado)");

        Debug.Log($"[SaludDelJugador] >>> CARGANDO ESCENA 0 (primerMundo) desde {SceneManager.GetActiveScene().name} <<<");
        
        // SIEMPRE volver al primer nivel (escena 0 - primerMundo)
        SceneManager.LoadScene(0);
    }
}
