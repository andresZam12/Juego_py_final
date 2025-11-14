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

        Debug.Log("Jugador recibe daño. Vida actual: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("El jugador ha muerto");

        // Mostrar GAME OVER panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Bloquear controles del jugador aquí si tienes script de movimiento
        // GetComponent<PlayerMovement>().enabled = false;

        // Reiniciar la escena después de 3 segundos
        Invoke("RestartScene", 3f);
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
