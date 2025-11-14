using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class WarrokEnemyBar : MonoBehaviour
{
    [Header("Referencias")]
    public WarrokEnemy enemy;                  // Referencia al script del enemigo (root)
    public Image healthBarFill;                // Imagen que representa el fill de la barra
    public TextMeshProUGUI healthText;         // Texto que muestra la vida numérica
    public Canvas canvas;                      // Canvas que contiene la barra de vida

    private float lastHealth = -1f;            // Para detectar cambios en la vida

    void Start()
    {
        // Buscar referencias si no están asignadas
        if (enemy == null)
            enemy = GetComponentInParent<WarrokEnemy>();

        if (canvas == null)
            canvas = GetComponent<Canvas>();

        // Configurar el canvas para renderizar en el mundo 3D
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            if (Camera.main != null)
                canvas.worldCamera = Camera.main;
        }

        // Inicializar valores seguros
        if (enemy != null)
            lastHealth = enemy.GetHealth();
        else
            lastHealth = -1f;

        UpdateUI(); // Actualizar la interfaz al iniciar
    }

    void Update()
    {
        // Hacer que el canvas mire hacia la cámara (si está configurado)
        if (canvas != null && Camera.main != null)
        {
            // orientamos el canvas hacia la cámara sin invertir la rotación
            Vector3 lookDir = Camera.main.transform.position - canvas.transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.0001f)
                canvas.transform.rotation = Quaternion.LookRotation(-lookDir); // mirar hacia la cámara
        }

        // Solo actualizar la UI si la vida ha cambiado
        if (enemy == null) return;

        float currentHealth = enemy.GetHealth();
        if (!Mathf.Approximately(currentHealth, lastHealth))
        {
            UpdateUI();
            lastHealth = currentHealth;
        }
    }

    private void UpdateUI()
    {
        if (enemy == null) return;

        int cur = enemy.GetHealth();
        int max = enemy.GetMaxHealth();
        if (max <= 0) return;

        // Actualizar el texto de vida
        if (healthText != null)
        {
            int vidaEntera = Mathf.Clamp(cur, 0, max);
            healthText.text = vidaEntera.ToString();
        }

        // Actualizar el fill de la barra de vida
        if (healthBarFill != null)
        {
            float healthPercentage = (float)cur / (float)max;
            healthBarFill.fillAmount = Mathf.Clamp01(healthPercentage);
        }
    }
}
