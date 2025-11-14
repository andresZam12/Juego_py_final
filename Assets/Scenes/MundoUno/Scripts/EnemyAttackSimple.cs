using UnityEngine;

public class EnemyAttackSimple : MonoBehaviour
{
    public int attackDamage = 10;
    public float attackCooldown = 1.0f; // segundos entre ataques
    public float attackRange = 1.5f;    // distancia a la que puede golpear
    public bool requireFacing = true;   // si true, ataca solo si está mirando al player (opcional)

    private float lastAttackTime = -999f;
    Transform player;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            // opcional: comprobar si "mira" al player
            if (requireFacing)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dir); // 1 = directo delante
                if (dot < 0.3f) return; // no ataca si no está aproximadamente mirando al player
            }

            TryAttack(player.gameObject);
        }
    }

    void TryAttack(GameObject playerObj)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        var ph = playerObj.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(attackDamage);
            Debug.Log($"{name} attacked player for {attackDamage}");
        }
    }
}
