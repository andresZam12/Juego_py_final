using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Referencias")]
    public Button playButton;
    public Button quitButton;

    void Start()
    {
        // Configurar botones
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        else
            Debug.LogWarning("Falta asignar el botón Play en el inspector.");

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        else
            Debug.LogWarning("Falta asignar el botón Quit en el inspector.");
    }

    private void OnPlayClicked()
    {
        // ✅ CORRECCIÓN: Solo llamar al LevelManager
        // El LevelManager se encargará de ocultar el menú
        if (LevelManager.instance != null)
        {
            LevelManager.instance.StartGame();
        }
        else
        {
            Debug.LogError("LevelManager instance no encontrada!");
        }
    }

    private void OnQuitClicked()
    {
        Debug.Log("Saliendo del juego...");
        if (LevelManager.instance != null)
            LevelManager.instance.QuitGame();
        else
            Application.Quit();
    }
}
