using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Tooltip("Índices: 0 = mundoUno, 1 = mundoDos, 2 = mundoTres")]
    public int[] worldScores = new int[3];
    
    // Cache para ScoreUI - evitar FindObjectOfType repetidas
    private ScoreUI cachedScoreUI;

    public int TotalScore
    {
        get
        {
            int s = 0;
            for (int i = 0; i < worldScores.Length; i++) s += worldScores[i];
            return s;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("GameManager Awake - instance set");
    }

    public void AddScore(int amount)
    {
        int i = SceneManager.GetActiveScene().buildIndex;
        if (i >= 0 && i < worldScores.Length)
        {
            worldScores[i] += amount;
        }

        // Update UI using cached reference
        if (cachedScoreUI == null)
            cachedScoreUI = FindObjectOfType<ScoreUI>();
        
        if (cachedScoreUI != null) 
            cachedScoreUI.UpdateScoreText();
    }

    public void ResetScores()
    {
        for (int j = 0; j < worldScores.Length; j++) worldScores[j] = 0;
        
        if (cachedScoreUI == null)
            cachedScoreUI = FindObjectOfType<ScoreUI>();
        
        if (cachedScoreUI != null) 
            cachedScoreUI.UpdateScoreText();
    }

    public void GoToNextWorld()
    {
        cachedScoreUI = null; // Limpiar cache al cambiar escena
        
        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;

        if (next < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(next);
        }
        else
        {
            // If no more worlds, go to end screen
            SceneManager.LoadScene(3);
        }
    }

    public void GoToFirstWorld()
    {
        cachedScoreUI = null; // Limpiar cache al cambiar escena
        ResetScores();
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
