using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    // Nombre de la escena donde empieza el juego (cámbialo por el tuyo)
    public string nombreEscenaJuego = "primerMundo";

    // Llamar a esto desde el botón Jugar
    public void Jugar()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    // Opcional: botón salir (funciona en build, no en el editor)
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
