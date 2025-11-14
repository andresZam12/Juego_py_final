using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Tooltip("Índices: 0 = mundoUno, 1 = mundoDos, 2 = mundoTres")]
    public int[] worldScores = new int[3];

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
            Debug.Log($"GameManager.AddScore: +{amount} on scene {i} => worldScores[{i}] = {worldScores[i]}  total={TotalScore}");
        }
        else
        {
            Debug.Log($"GameManager.AddScore called on scene index {i} (not a world). Amount: {amount}");
        }

        // Update UI if present
        var ui = FindObjectOfType<ScoreUI>();
        if (ui != null) ui.UpdateScoreText();
    }

    public void ResetScores()
    {
        for (int j = 0; j < worldScores.Length; j++) worldScores[j] = 0;
        Debug.Log("GameManager.ResetScores()");
        var ui = FindObjectOfType<ScoreUI>();
        if (ui != null) ui.UpdateScoreText();
    }

    public void GoToNextWorld()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;
        Debug.Log($"GameManager.GoToNextWorld: current={current} next={next}");

        if (next < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(next);
        }
        else
        {
            // If no more worlds, go to end screen (assume build index 3 or handle as needed)
            SceneManager.LoadScene(3);
        }
    }

    public void GoToFirstWorld()
    {
        ResetScores();
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("GameManager.QuitGame called");
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
