using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    public void RestartGame()
    {
        // Carga la primera escena (índice 0) o puedes poner el nombre de la escena del menú
        SceneManager.LoadScene(0); // Esto carga la primera escena en Build Settings
    }
}