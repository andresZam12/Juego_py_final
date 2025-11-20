using UnityEngine;
using UnityEngine.UI;

public class VidaPlayer : MonoBehaviour
{
    public float health = 100f;
    public Slider healthBar;

    void Start()
    {
        // Inicializar la barra de vida
        if (healthBar != null)
        {
            healthBar.maxValue = 1f;
            healthBar.value = 1f;
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        
        // Actualizar barra de vida
        if (healthBar != null)
        {
            healthBar.value = Mathf.Clamp01(health / 100f);
        }
        
        Debug.Log("❤️ Jugador recibió " + amount + " de daño. Vida restante: " + health);

        // Verificar si murió
        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("💀💀💀 JUGADOR MUERTO 💀💀💀");
        
        // Llamar al NivelManager para mostrar Game Over
        if (NivelManager.instance != null)
        {
            NivelManager.instance.ShowGameOver();
        }
        else
        {
            Debug.LogError("❌ NivelManager.instance es NULL - No se puede mostrar Game Over");
        }
    }
}
