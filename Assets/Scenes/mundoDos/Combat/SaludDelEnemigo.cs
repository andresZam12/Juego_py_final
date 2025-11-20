using UnityEngine;
using UnityEngine.SceneManagement;
using System; // necesario para Action<>
using System.Collections;

public class SaludDelEnemigo : MonoBehaviour
{
    [Header("Vida del enemigo")]
    public int maxHealth = 10;
    private int currentHealth;

    // Propiedad para UI externas
    public int CurrentHealth => currentHealth;

    [Header("UI / Win Panel")]
    public GameObject winPanel;      // asigna aquí tu WinPanel

    [Header("Transición al siguiente nivel")]
    public float delayBeforeNextLevel = 3f; // Tiempo para mostrar panel de victoria

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
        Debug.Log("¡Enemigo MUERTO! Iniciando transición al tercer nivel...");

        // Efecto de explosión
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, transform.rotation);

        // Mostrar cartel GANASTE
        if (winPanel != null)
            winPanel.SetActive(true);

        // Avisar al spawner u otros sistemas
        OnDeath?.Invoke(this);

        // Desactivar el enemigo visualmente pero NO destruirlo aún
        // para que la corrutina pueda completarse
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
            c.enabled = false;

        // Iniciar corrutina para cargar el siguiente nivel
        StartCoroutine(LoadNextLevelCoroutine());
    }

    private IEnumerator LoadNextLevelCoroutine()
    {
        Debug.Log("[SaludDelEnemigo] Esperando " + delayBeforeNextLevel + " segundos antes de cargar el tercer nivel...");
        
        // Esperar mientras se muestra el panel de victoria
        yield return new WaitForSeconds(delayBeforeNextLevel);

        Debug.Log("[SaludDelEnemigo] Cargando Map_v1 (índice 2)...");
        
        // Cargar el tercer nivel (Map_v1 está en el índice 2 del Build Settings)
        SceneManager.LoadScene(2);
        
        // Ahora sí, destruir el gameObject después de iniciar la carga
        if (destroyGameObject)
            Destroy(gameObject);
    }
}
