using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameOverPanel;  // Panel que contiene todo el UI de Game Over
    public Text gameOverText;         // Texto "GAME OVER"
    public Button restartButton;      // Botón de reiniciar

    void Start()
    {
        // Ocultar el panel al inicio
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Asignar la función al botón
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    // Llamar esta función cuando el jugador muera
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Pausar el juego (opcional)
        }
    }

    // Función para reiniciar el juego
    public void RestartGame()
    {
        Time.timeScale = 1f; // Restablecer el tiempo
        SceneManager.LoadScene(0); // Carga la primera escena (índice 0)
        
        // Alternativa: Cargar por nombre
        // SceneManager.LoadScene("NombreDeTuEscena");
    }

    // Función alternativa para cargar una escena específica
    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}


// ===== SCRIPT PARA EL JUGADOR (Ejemplo) =====
// Coloca este script en tu jugador o donde detectes la muerte

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;
    private GameOverManager gameOverManager;

    void Start()
    {
        // Buscar el GameOverManager en la escena
        gameOverManager = FindObjectOfType<GameOverManager>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("El jugador ha muerto");
        
        // Mostrar Game Over
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }

        // Desactivar controles del jugador (opcional)
        GetComponent<Rigidbody>().isKinematic = true;
        // o this.enabled = false;
    }
}