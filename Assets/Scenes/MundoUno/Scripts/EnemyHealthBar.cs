using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBarTres : MonoBehaviour
{
    public MutantEnemy enemy; // Referencia al script del enemigo
    public Image healthBarFill; // Imagen que representa el fill de la barra
    public TextMeshProUGUI healthText; // Texto que muestra la vida numérica
    public Canvas canvas; // Canvas que contiene la barra de vida

    private float lastHealth = -1f; // Para detectar cambios en la vida

    void Start()
    {
        // Buscar referencias si no están asignadas
        if (enemy == null)
            enemy = GetComponentInParent<MutantEnemy>();

        if (canvas == null)
            canvas = GetComponent<Canvas>();

        // Configurar el canvas para renderizar en el mundo 3D
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
        }

        UpdateUI(); // Actualizar la interfaz al iniciar
        lastHealth = enemy != null ? enemy.health : -1f;
    }

    void Update()
    {
        // Hacer que el canvas mire hacia la cámara
        if (canvas != null && Camera.main != null)
        {
            canvas.transform.LookAt(
                canvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }

        // Solo actualizar la UI si la vida ha cambiado
        if (enemy != null && enemy.health != lastHealth)
        {
            UpdateUI();
            lastHealth = enemy.health;
        }
    }

    private void UpdateUI()
    {
        if (enemy == null || enemy.maxHealth <= 0) return;

        // Actualizar el texto de vida
        if (healthText != null)
        {
            int vidaEntera = Mathf.RoundToInt(enemy.health);
            vidaEntera = Mathf.Clamp(vidaEntera, 0, Mathf.RoundToInt(enemy.maxHealth));
            healthText.text = vidaEntera.ToString();
        }

        // Actualizar el fill de la barra de vida
        if (healthBarFill != null)
        {
            float healthPercentage = enemy.health / enemy.maxHealth;
            healthBarFill.fillAmount = Mathf.Clamp01(healthPercentage);
        }
    }
}
