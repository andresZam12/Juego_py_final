using UnityEngine;
using UnityEngine.UI;

public class VidaEnemy : MonoBehaviour
{
    public SkeletonEnemy enemy; // referencia al enemigo
    public Slider slider;
    public Text valueText; // opcional: indicador numérico

    void Start()
    {
        if (enemy == null)
            enemy = GetComponentInParent<SkeletonEnemy>();
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
        }
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (enemy == null || slider == null) return;
        float normalized = Mathf.Clamp01(enemy.health / 100f);
        slider.value = normalized;
        if (valueText != null)
        {
            int hpInt = Mathf.CeilToInt(enemy.health);
            valueText.text = hpInt.ToString();
        }
    }
    
}