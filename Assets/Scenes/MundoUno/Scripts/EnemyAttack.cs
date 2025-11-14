using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyAttack : MonoBehaviour
{
    public int attackDamage = 10;
    public float attackCooldown = 1.0f; // segundos entre ataques
    private float lastAttackTime = -999f;

    // Opción A: usar trigger en la parte frontal del enemigo
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryAttack(other.gameObject);
        }
    }

    // Opción B: usar collision (si enemigo tiene Rigidbody y collider)
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            TryAttack(collision.collider.gameObject);
        }
    }

    void TryAttack(GameObject player)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        var ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(attackDamage);
            Debug.Log($"{name} attacked player for {attackDamage}");
        }
    }
}
