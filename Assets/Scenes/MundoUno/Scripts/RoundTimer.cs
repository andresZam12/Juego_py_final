using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RoundTimer : MonoBehaviour
{
    [Header("Timer")]
    public float timeSeconds = 20f;
    public int requiredScore = 100;

    [Header("UI")]
    public TextMeshProUGUI timerText;       // asignar en Inspector
    public TextMeshProUGUI messageText;     // opcional: "Game Over"

    bool running = true;
    bool resultHandled = false; // evita ejecuciones dobles de CheckResult

    void Start()
    {
        if (timerText == null)
            Debug.LogWarning("RoundTimer: timerText no asignado.");

        // asegurar que el mensaje esté oculto al inicio
        if (messageText != null)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(false);
        }

        UpdateUI();
    }

    void Update()
    {
        if (!running) return;

        timeSeconds -= Time.deltaTime;
        if (timeSeconds <= 0f)
        {
            timeSeconds = 0f;
            running = false;
            UpdateUI();
            CheckResult();
        }
        else
        {
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(timeSeconds)}";
    }

    void CheckResult()
    {
        if (resultHandled) return;
        resultHandled = true;

        int score = ReadScoreFromGameManager();

        if (score >= requiredScore)
        {
            // Pasar al siguiente nivel
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToNextWorld();
            }
        }
        else
        {
            // Game Over
            if (messageText != null)
            {
                messageText.gameObject.SetActive(true);
                messageText.text = "Game Over";
            }

            Time.timeScale = 0f;
            StartCoroutine(GameOverAndRestartCoroutine(10f));
        }
    }

    int ReadScoreFromGameManager()
    {
        // Acceso directo sin reflection - mucho más eficiente
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.TotalScore;
        }
        
        return 0;
    }

    /// <summary>
    /// Coroutine de Game Over del RoundTimer.
    /// SIEMPRE devuelve al jugador al primer nivel (escena 0 - primerMundo).
    /// </summary>
    private System.Collections.IEnumerator GameOverAndRestartCoroutine(float seconds)
    {
        float t = seconds;
        while (t > 0f)
        {
            if (messageText != null)
                messageText.text = $"Game Over\nReiniciando en {Mathf.CeilToInt(t)}";
            yield return new WaitForSecondsRealtime(1f);
            t -= 1f;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScores();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
