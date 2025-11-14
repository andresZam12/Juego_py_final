using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        UpdateScoreText();
    }

    void OnEnable()
    {
        // también actualizar cuando el objeto se active (por ejemplo al cambiar de escena)
        UpdateScoreText();
    }

    /// <summary>
    /// Método público que GameManager (u otros) puede invocar para forzar la actualización del texto.
    /// </summary>
    public void UpdateScoreText()
    {
        if (scoreText == null)
            return;

        if (GameManager.Instance != null)
            scoreText.text = "Score: " + GameManager.Instance.TotalScore;
        else
            scoreText.text = "Score: 0";
    }

    // Nota: no necesitamos actualizar cada frame. Si quieres refrescar en tiempo real, 
    // puedes descomentar Update() y dejarlo. Pero es más eficiente llamar UpdateScoreText()
    // sólo cuando el score cambia (GameManager.AddScore hace un Find+Update).
    /*
    void Update()
    {
        UpdateScoreText();
    }
    */
}
