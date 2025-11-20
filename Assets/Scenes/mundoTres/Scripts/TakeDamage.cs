using UnityEngine;

public class TakeDamage : MonoBehaviour
{
    public float damageAmount = 25f;
    private VidaPlayer vidaPlayer;

    void Start()
    {
        // Buscar VidaPlayer en este mismo GameObject (lo más común)
        vidaPlayer = GetComponent<VidaPlayer>();

        // Si VidaPlayer está en un hijo (ej. "Player/HealthSystem"), usa:
        // vidaPlayer = GetComponentInChildren<VidaPlayer>();

        // Si está en el padre:
        // vidaPlayer = GetComponentInParent<VidaPlayer>();

        if (vidaPlayer == null)
        {
            Debug.LogError("❌ No se encontró VidaPlayer en " + gameObject.name);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet") || other.CompareTag("Obstacle"))
        {
            Debug.Log($"💥 Colisión con {other.name} → aplicando {damageAmount} daño");

            if (vidaPlayer != null)
            {
                vidaPlayer.TakeDamage(damageAmount); // ✅ ¡Aquí se conectan!
            }
        }
    }

    // También puedes usar OnCollisionEnter2D si usas colisiones físicas
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"💥 Colisión física con {col.gameObject.name} → daño: {damageAmount}");
            vidaPlayer?.TakeDamage(damageAmount);
        }
    }
}