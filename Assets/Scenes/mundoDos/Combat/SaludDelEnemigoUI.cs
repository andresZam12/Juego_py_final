using UnityEngine;
using UnityEngine.UI;

public class SaludDelEnemigoUI : MonoBehaviour
{
    [Header("Referencias")]
    public SaludDelEnemigo enemigo;   // antes EnemyHealth
    public Slider slider;

    private void Start()
    {
        // Si no se asigna desde el Inspector, intentar buscar el componente en el padre
        if (enemigo == null)
        {
            enemigo = GetComponentInParent<SaludDelEnemigo>();
        }

        if (enemigo == null)
        {
            Debug.LogWarning("SaludDelEnemigoUI: no se encontró referencia a SaludDelEnemigo.");
            return;
        }

        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>();
        }

        if (slider != null)
        {
            slider.maxValue = enemigo.maxHealth;
            slider.value = enemigo.CurrentHealth;   // propiedad pública en SaludDelEnemigo
        }
    }

    private void Update()
    {
        if (enemigo == null || slider == null) return;

        // Actualizar la barra de vida en tiempo real
        slider.value = enemigo.CurrentHealth;
    }
}
