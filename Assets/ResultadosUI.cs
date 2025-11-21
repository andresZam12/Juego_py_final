using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultadosUI : MonoBehaviour
{
    [Header("Textos de Mundos")]
    public TextMeshProUGUI textoMundo1;
    public TextMeshProUGUI textoMundo2;
    public TextMeshProUGUI textoMundo3;

    [Header("Texto de Total")]
    public TextMeshProUGUI textoTotal;

    [Header("Puntajes de cada Mundo")]
    public int scoreMundo1;
    public int scoreMundo2;
    public int scoreMundo3;

    private int scoreTotal;

    void Start()
    {
        // Si quieres, aquí puedes cargar los valores desde PlayerPrefs u otro lado
        CalcularYMostrarScores();
    }

    public void CalcularYMostrarScores()
    {
        scoreTotal = scoreMundo1 + scoreMundo2 + scoreMundo3;

        if (textoMundo1 != null)
            textoMundo1.text = "Mundo 1: " + scoreMundo1;

        if (textoMundo2 != null)
            textoMundo2.text = "Mundo 2: " + scoreMundo2;

        if (textoMundo3 != null)
            textoMundo3.text = "Mundo 3: " + scoreMundo3;

        if (textoTotal != null)
            textoTotal.text = "Total: " + scoreTotal;
    }

    // Llama a esta función desde el botón Repetir
    public void RepetirJuego()
    {
        // Cambia "NombreDeTuPrimeraEscena" por el nombre real de tu escena del juego
        SceneManager.LoadScene("NombreDeTuPrimeraEscena");
    }

    // Si desde otros scripts quieres actualizar los scores:
    public void SetScores(int mundo1, int mundo2, int mundo3)
    {
        scoreMundo1 = mundo1;
        scoreMundo2 = mundo2;
        scoreMundo3 = mundo3;
        CalcularYMostrarScores();
    }
}
