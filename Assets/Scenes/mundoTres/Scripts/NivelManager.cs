using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NivelManager : MonoBehaviour
{
    public static NivelManager instance;
    
    [Header("Sistema de Enemigos")]
    private int totalEnemies = 0;
    private int enemiesKilled = 0;
    
    [Header("Paneles UI - ARRASTRA AQUÍ")]
    public GameObject gameOverPanel; // Arrastra GameOverPanel aquí
    public GameObject victoryPanel;  // Arrastra Win_Panel aquí
    
    private bool gameEnded = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        gameEnded = false;
        
        // Contar enemigos al inicio
        totalEnemies = FindObjectsOfType<SkeletonEnemy>().Length;
        enemiesKilled = 0;
        
        // IMPORTANTE: Ocultar paneles al inicio
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("✅ GameOverPanel ocultado");
        }
        else
        {
            Debug.LogError("❌ Game Over Panel NO asignado en el Inspector");
        }
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
            Debug.Log("✅ Win_Panel ocultado");
        }
        else
        {
            Debug.LogError("❌ Victory Panel NO asignado en el Inspector");
        }
            
        Debug.Log("🎯 Enemigos totales: " + totalEnemies);
    }

    // Llamar cuando un enemigo muera
    public void EnemyKilled()
    {
        if (gameEnded)
            return;
            
        enemiesKilled++;
        Debug.Log("💀 Enemigos eliminados: " + enemiesKilled + "/" + totalEnemies);
        
        // Verificar si mataste a TODOS
        if (enemiesKilled >= totalEnemies)
        {
            ShowVictory();
        }
    }

    // Mostrar Game Over (cuando el jugador muere)
    public void ShowGameOver()
    {
        if (gameEnded)
            return;
            
        gameEnded = true;
        Time.timeScale = 0f;

        Debug.Log("💀 Mostrando Game Over");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ No se puede mostrar Game Over - Panel no asignado");
            Time.timeScale = 1f;
            SceneManager.LoadScene("GameOver");
        }
    }

    // Mostrar Victoria (cuando matas a todos los enemigos)
    void ShowVictory()
    {
        if (gameEnded)
            return;
            
        gameEnded = true;
        Time.timeScale = 1f; // Reanudamos para que la corrutina funcione

        Debug.Log("🏆 ¡VICTORIA! Mostrando Win_Panel");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ No se puede mostrar Victoria - Panel no asignado");
        }
        
        // Iniciar corrutina para cargar ResultadosEscene después de 3 segundos
        StartCoroutine(LoadResultsSceneAfterDelay(3f));
    }
    
    // Corrutina para cargar la escena de resultados
    private IEnumerator LoadResultsSceneAfterDelay(float delay)
    {
        Debug.Log("⏱️ Esperando " + delay + " segundos antes de cargar ResultadosEscene...");
        
        yield return new WaitForSeconds(delay);
        
        Debug.Log("🎬 Cargando ResultadosEscene (índice 4)...");
        // ResultadosEscene está en el índice 4 del Build Settings
        SceneManager.LoadScene(4);
    }

    // ===== FUNCIONES PARA BOTONES =====
    
    // Botón "REINICIAR" (RestartButton)
    public void ReiniciarNivel()
    {
        Debug.Log("🔄 Reiniciando nivel...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Botón "MENÚ" (Win_Button)
    public void GoToMenu()
    {
        Debug.Log("📋 Volviendo al menú...");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu"); // ⚠️ Cambia "Menu" por el nombre de tu escena
    }

    // Botón alternativo "JUGAR DE NUEVO"
    public void PlayAgain()
    {
        ReiniciarNivel();
    }
}

