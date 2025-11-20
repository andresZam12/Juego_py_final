using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryButton : MonoBehaviour
{
    // Función para volver al menú principal
    public void GoToMenu()
    {
        Time.timeScale = 1f; // Asegurar que el tiempo esté normal
        SceneManager.LoadScene("Menu"); // Cambia "Menu" por el nombre exacto de tu escena de menú
    }

    // Función alternativa si tu menú se llama diferente
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Carga la primera escena (normalmente el menú)
    }

    // Función para reintentar el nivel actual
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene"); // Tu escena del juego
    }

}