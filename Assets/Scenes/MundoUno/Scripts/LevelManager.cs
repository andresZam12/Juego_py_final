using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// LevelManager (corregido) - versión final
/// - Incluye StartGameSession() correctamente definido.
/// - Robustez para buscar el messageText y mainMenuPanel en la escena.
/// - Secuencia de Game Over por muerte del jugador (conteo en real time).
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Level")]
    public int levelScore;
    public float levelTimer = 60f;
    public string levelName1 = "Aeropuerto";
    public string levelName2 = "Desierto";

    [Header("Estado del juego")]
    public bool isGameActive = false;
    public GameObject mainMenuPanel;          // opcional: asignar en inspector

    [Header("Mensajes / UI")]
    public TextMeshProUGUI messageText;      // asignar en inspector o será buscado
    public float messageDuration = 2f;

    private static bool hasSessionStarted = false;
    private bool levelEndTriggered = false;
    private PlayerHealthMain playerHealth;

    void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        levelScore = 0;
        playerHealth = FindObjectOfType<PlayerHealthMain>();

        if (messageText != null)
        {
            messageText.text = string.Empty;
            messageText.gameObject.SetActive(false);
        }

        // Si no hay panel de menú en inspector, arrancamos la sesión automáticamente
        if (mainMenuPanel == null)
        {
            hasSessionStarted = true;
            StartGameSession();
            return;
        }

        if (!hasSessionStarted)
            ShowMainMenu();
        else
            StartGameSession();
    }

    void Update()
    {
        if (!isGameActive) return;

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == levelName1)
            HandleLevel1();
        else if (currentScene == levelName2)
            HandleLevel2();

        // Verificar muerte del jugador
        CheckPlayerDeath();
    }

    private void CheckPlayerDeath()
    {
        if (playerHealth != null && playerHealth.GetHealth() <= 0 && !levelEndTriggered)
        {
            // iniciar secuencia de game over por muerte del jugador
            levelEndTriggered = true;
            Debug.Log($"LevelManager: Detectada muerte del jugador en escena {SceneManager.GetActiveScene().name} - Iniciando retorno al primer nivel");
            StartCoroutine(PlayerDeathReturnToFirstWorldCoroutine(10f));
        }
    }

    #region Public API

    public void OnEnemyKilled(int scoreForKill = 1)
    {
        if (!isGameActive || levelEndTriggered) return;
        levelScore += scoreForKill;
        Debug.Log($"Enemigo eliminado! Score: {levelScore}");
    }

    public void StartGame()
    {
        hasSessionStarted = true;
        levelEndTriggered = false;
        levelScore = 0;
        levelTimer = 60f;

        StartGameSession();
        Debug.Log("StartGame() called by UI or code.");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowMainMenu()
    {
        isGameActive = false;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #endregion

    #region Session control

    // <-- THIS IS THE METHOD THAT WAS MISSING PREVIOUSLY -->
    private void StartGameSession()
    {
        isGameActive = true;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        // Hide message if present
        if (messageText != null)
            messageText.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealthMain>();

        Debug.Log("StartGameSession: game started, timescale=" + Time.timeScale);
    }

    #endregion

    #region Level handlers

    private void HandleLevel1()
    {
        if (levelEndTriggered) return;

        if (levelScore < 4)
        {
            if (levelTimer > 0f)
                levelTimer -= Time.deltaTime;
            else if (!levelEndTriggered)
            {
                levelEndTriggered = true;
                Debug.Log("Nivel 1: Tiempo agotado - iniciando GameOverSequence");
                StartCoroutine(GameOverSequence());
            }
        }
        else if (!levelEndTriggered)
        {
            levelEndTriggered = true;
            ShowMessage("Aeropuerto Completado!");
            StartCoroutine(LoadAfterDelay(levelName2, messageDuration));
        }
    }

    private void HandleLevel2()
    {
        if (levelEndTriggered) return;

        bool levelCompleted = CheckLevel2Completion();
        if (levelCompleted && !levelEndTriggered)
        {
            levelEndTriggered = true;
            StartCoroutine(ShowLevelCompletedThenMenu());
        }
    }

    private bool CheckLevel2Completion()
    {
        WarrokEnemy warrokEnemy = FindObjectOfType<WarrokEnemy>();
        if (warrokEnemy != null)
            return IsWarrokEnemyDefeated(warrokEnemy);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
            return AreAllEnemiesDefeated(enemies);

        return true;
    }

    private bool IsWarrokEnemyDefeated(WarrokEnemy enemy)
    {
        if (enemy == null) return true;
        try
        {
            if (!enemy.gameObject.activeInHierarchy) return true;
            if (!enemy.enabled) return true;
            if (enemy.IsDead()) return true;
            if (enemy.GetHealth() <= 0) return true;
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error verificando WarrokEnemy: " + e.Message);
            return true;
        }
    }

    private bool AreAllEnemiesDefeated(GameObject[] enemies)
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy.activeInHierarchy)
            {
                WarrokEnemy warrok = enemy.GetComponent<WarrokEnemy>();
                if (warrok != null && !warrok.IsDead()) return false;

                MutantEnemy mutant = enemy.GetComponent<MutantEnemy>();
                if (mutant != null && mutant.enabled) return false;

                MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
                foreach (var script in scripts)
                {
                    if (script != null && script.enabled &&
                        script.GetType() != typeof(Animator) &&
                        script.GetType() != typeof(Transform))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    #endregion

    #region Game Over & transitions

    /// <summary>
    /// Game Over por tiempo agotado en nivel 1.
    /// SIEMPRE devuelve al jugador al primer nivel para reiniciar el juego completo.
    /// </summary>
    private IEnumerator GameOverSequence()
    {
        ShowMessage("Game Over - Tiempo agotado");

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(messageDuration);

        Time.timeScale = 1f;
        levelEndTriggered = false;
        levelScore = 0;
        levelTimer = 60f;

        // Resetear scores del GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.ResetScores();

        // SIEMPRE volver al primer nivel (escena 0 - primerMundo)
        Debug.Log("GameOver por timeout: Cargando escena 0 (primerMundo)");
        SceneManager.LoadScene(0);

        yield return new WaitForSeconds(0.1f);
        
        if (mainMenuPanel != null)
            ShowMainMenu();
    }

    private IEnumerator ShowLevelCompletedThenMenu()
    {
        ShowMessage("¡Juego Completado!");
        yield return new WaitForSeconds(messageDuration);
        ShowMainMenu();
    }

    private void ShowMessage(string msg)
    {
        if (messageText == null)
            messageText = FindMessageTextInScene();

        if (messageText != null)
        {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay());
        }
        Debug.Log(msg);
    }

    private IEnumerator LoadAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);

        if (sceneName == levelName1)
        {
            levelScore = 0;
            levelTimer = 60f;
        }
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Inicia la secuencia de Game Over cuando el jugador muere.
    /// SIEMPRE devuelve al jugador al primer nivel (escena 0 - primerMundo) después del countdown.
    /// Funciona en CUALQUIER nivel donde muera el jugador.
    /// </summary>
    /// <param name="countdownSeconds">Tiempo en segundos antes de reiniciar (default: 10)</param>
    public void TriggerPlayerDeathReturnToFirstWorld(float countdownSeconds = 10f)
    {
        Debug.Log($"TriggerPlayerDeathReturnToFirstWorld llamado en escena: {SceneManager.GetActiveScene().name}");
        if (!levelEndTriggered)
        {
            levelEndTriggered = true;
            Debug.Log("Iniciando PlayerDeathReturnToFirstWorldCoroutine");
            StartCoroutine(PlayerDeathReturnToFirstWorldCoroutine(countdownSeconds));
        }
        else
        {
            Debug.LogWarning("TriggerPlayerDeathReturnToFirstWorld: levelEndTriggered ya estaba en true, ignorando");
        }
    }

    /// <summary>
    /// Coroutine que maneja el countdown de Game Over y retorna al primer nivel.
    /// - Muestra mensaje "Game Over" con countdown
    /// - Pausa el juego (Time.timeScale = 0)
    /// - Resetea todos los scores
    /// - Carga la escena 0 (primerMundo)
    /// </summary>
    private IEnumerator PlayerDeathReturnToFirstWorldCoroutine(float seconds)
    {
        Debug.Log($"PlayerDeathReturnToFirstWorldCoroutine iniciada - Escena actual: {SceneManager.GetActiveScene().name}");
        
        if (messageText == null)
            messageText = FindMessageTextInScene();

        if (messageText != null) messageText.gameObject.SetActive(true);

        Time.timeScale = 0f;
        Debug.Log("Time.timeScale = 0 (juego pausado)");
        
        float t = seconds;
        while (t > 0f)
        {
            if (messageText != null)
                messageText.text = $"Game Over\nReiniciando en {Mathf.CeilToInt(t)}";
            yield return new WaitForSecondsRealtime(1f);
            t -= 1f;
        }

        Time.timeScale = 1f;
        Debug.Log("Time.timeScale = 1 (juego reanudado)");
        
        levelEndTriggered = false;
        levelScore = 0;
        levelTimer = 60f;
        hasSessionStarted = false;

        if (GameManager.Instance != null)
        {
            Debug.Log("Reseteando scores del GameManager");
            GameManager.Instance.ResetScores();
        }

        Debug.Log($">>> CARGANDO ESCENA 0 (primerMundo) desde escena actual: {SceneManager.GetActiveScene().name} <<<");
        SceneManager.LoadScene(0);
        yield return null;

        if (mainMenuPanel == null)
            mainMenuPanel = FindMainMenuPanelInScene();

        if (mainMenuPanel != null)
            ShowMainMenu();
    }

    #endregion

    #region Helpers

    TextMeshProUGUI FindMessageTextInScene()
    {
        GameObject go = GameObject.Find("TimerMessage") ?? GameObject.Find("MessageText") ?? GameObject.Find("TimerText");
        if (go != null)
        {
            var t = go.GetComponent<TextMeshProUGUI>();
            if (t != null) return t;
            var child = go.GetComponentInChildren<TextMeshProUGUI>();
            if (child != null) return child;
        }

        var all = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach (var tm in all)
        {
            string n = tm.gameObject.name.ToLower();
            if (n.Contains("game") || n.Contains("timer") || n.Contains("message"))
                return tm;
        }

        if (all.Length > 0) return all[0];
        return null;
    }

    GameObject FindMainMenuPanelInScene()
    {
        GameObject go = GameObject.Find("MainMenuPanel");
        if (go != null) return go;

        go = GameObject.Find("MainMenu");
        if (go != null) return go;

        try
        {
            GameObject byTag = GameObject.FindWithTag("MainMenu");
            if (byTag != null) return byTag;
        }
        catch { }

        var all = GameObject.FindObjectsOfType<GameObject>();
        foreach (var obj in all)
        {
            string n = obj.name.ToLower();
            if (n.Contains("mainmenu") || n.Contains("menu") || n.Contains("main_menu"))
                return obj;
        }

        return null;
    }

    #endregion

    #region Scene events

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerHealth = FindObjectOfType<PlayerHealthMain>();
        levelEndTriggered = false;

        if (messageText == null)
            messageText = FindMessageTextInScene();

        if (mainMenuPanel == null)
            mainMenuPanel = FindMainMenuPanelInScene();

        if (scene.name == levelName1)
        {
            levelScore = 0;
            levelTimer = 60f;
            Debug.Log("Cargando Nivel 1 - Sistema de 4 enemigos");
        }
        else if (scene.name == levelName2)
        {
            Debug.Log("Cargando Nivel 2 - Sistema de enemigos jefe");
        }

        if (isGameActive)
        {
            Time.timeScale = 1f;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion
}
