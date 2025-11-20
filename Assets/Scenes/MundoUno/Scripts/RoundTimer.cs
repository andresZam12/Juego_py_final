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
        if (resultHandled)
        {
            Debug.Log("RoundTimer: CheckResult called but already handled.");
            return;
        }
        resultHandled = true;

        int score = ReadScoreFromGameManager();
        Debug.Log($"RoundTimer: CHECK RESULT -> score={score}, required={requiredScore}");

        if (score >= requiredScore)
        {
            // PASAR AL SIGUIENTE NIVEL (sin mostrar "You Win")
            Debug.Log("RoundTimer: DECISION => PASS TO NEXT WORLD (score >= requiredScore)");
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToNextWorld();
            }
            else
            {
                Debug.LogWarning("RoundTimer: GameManager.Instance es null. No se puede cambiar escena.");
            }
        }
        else
        {
            // LOSE: mostrar "Game Over", pausar y reiniciar tras 10s (tiempo real)
            Debug.Log("RoundTimer: DECISION => LOSE (score < requiredScore)");
            if (messageText != null)
            {
                messageText.gameObject.SetActive(true);
                messageText.text = "Game Over";
            }

            // Pausar juego
            Time.timeScale = 0f;

            // iniciar coroutine que usa tiempo real para reiniciar
            StartCoroutine(GameOverAndRestartCoroutine(10f));
        }
    }

    int ReadScoreFromGameManager()
    {
        int score = 0;
        if (GameManager.Instance != null)
        {
            var gm = GameManager.Instance;

            // 1) Intentar propiedad TotalScore
            var prop = gm.GetType().GetProperty("TotalScore");
            if (prop != null)
            {
                object val = prop.GetValue(gm);
                if (val is int) return (int)val;
            }

            // 2) Intentar método GetTotalScore()
            var method = gm.GetType().GetMethod("GetTotalScore");
            if (method != null)
            {
                object mval = method.Invoke(gm, null);
                if (mval is int) return (int)mval;
            }

            // 3) Intentar array/field worldScores
            var field = gm.GetType().GetField("worldScores", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                object arr = field.GetValue(gm);
                if (arr is int[])
                {
                    foreach (var v in (int[])arr) score += v;
                    return score;
                }
            }

            // 4) Intentar campo público TotalScore (fallback)
            var fld = gm.GetType().GetField("TotalScore", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (fld != null)
            {
                object fv = fld.GetValue(gm);
                if (fv is int) return (int)fv;
            }

            Debug.LogWarning("RoundTimer: no pude leer el score desde GameManager (prop/method/field). Revisa GameManager.");
        }
        else
        {
            Debug.LogWarning("RoundTimer: GameManager.Instance es null — no puedo leer score.");
        }

        return score;
    }

    /// <summary>
    /// Coroutine de Game Over del RoundTimer.
    /// SIEMPRE devuelve al jugador al primer nivel (escena 0 - primerMundo).
    /// </summary>
    private System.Collections.IEnumerator GameOverAndRestartCoroutine(float seconds)
    {
        Debug.Log($"RoundTimer: GameOver coroutine started en escena: {SceneManager.GetActiveScene().name}");
        float t = seconds;
        while (t > 0f)
        {
            if (messageText != null)
                messageText.text = $"Game Over\nReiniciando en {Mathf.CeilToInt(t)}";
            yield return new WaitForSecondsRealtime(1f);
            t -= 1f;
        }

        // Resetear GameManager
        if (GameManager.Instance != null)
        {
            Debug.Log("RoundTimer: Reseteando GameManager.ResetScores()");
            GameManager.Instance.ResetScores();
        }

        Debug.Log($"RoundTimer: >>> CARGANDO ESCENA 0 (primerMundo) desde: {SceneManager.GetActiveScene().name} <<<");
        Time.timeScale = 1f;
        
        // SIEMPRE volver al primer nivel (escena 0 - primerMundo)
        SceneManager.LoadScene(0);
    }
}
