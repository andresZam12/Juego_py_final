using UnityEngine;
using System; // necesario para Action<>

public class SaludDelEnemigo : MonoBehaviour
{
    [Header("Vida del enemigo")]
    public int maxHealth = 10;
    private int currentHealth;

    // Propiedad para UI externas
    public int CurrentHealth => currentHealth;

    [Header("UI / Win Panel")]
    public GameObject winPanel;      // asigna aquí tu WinPanel

    [Header("FX")]
    public GameObject explosionEffect;
    public bool destroyGameObject = true;

    // 🔔 Evento con referencia al propio enemigo
    public event Action<SaludDelEnemigo> OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;

        if (winPanel != null)
            winPanel.SetActive(false);  // panel oculto al inicio
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemigo recibe daño. Vida actual: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("¡Enemigo MUERTO!");

        // Efecto de explosión
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, transform.rotation);

        // Mostrar cartel GANASTE
        if (winPanel != null)
            winPanel.SetActive(true);

        // Avisar al spawner u otros sistemas
        OnDeath?.Invoke(this);

        if (destroyGameObject)
            Destroy(gameObject);
    }
}
