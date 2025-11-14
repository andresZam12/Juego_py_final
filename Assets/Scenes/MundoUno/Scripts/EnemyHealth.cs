using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Tooltip("Si >0, el enemigo morirá después de este número de impactos (independiente del 'damage')")]
    public int hitsToDie = 5;             // <- valor por defecto ahora es 5

    public int maxHealth = 100;
    public int scoreOnDeath = 50;
    public float destroyDelay = 0.1f;     // desaparecer casi instantáneamente
    public string deathTrigger = "Die";

    int current;
    int hitsTaken = 0;
    bool dead = false;

    void Awake()
    {
        current = maxHealth;
    }

    // Cuenta impactos (usar TakeHit() por cada bala que impacte)
    public void TakeHit()
    {
        if (dead) return;
        hitsTaken++;
        Debug.Log($"{name} TakeHit: {hitsTaken}/{hitsToDie}");

        if (hitsToDie > 0 && hitsTaken >= hitsToDie)
            Die();
    }

    // Mantengo TakeDamage por compatibilidad; si hitsToDie>0 cuenta como 1 impacto
    public void TakeDamage(int amount)
    {
        if (dead) return;
        current -= amount;
        Debug.Log($"{name} TakeDamage: {amount} -> {current}");

        if (hitsToDie > 0)
        {
            // contar como 1 impacto y salir
            TakeHit();
            return;
        }

        if (current <= 0) Die();
    }

    void Die()
    {
        if (dead) return;
        dead = true;

        Debug.Log($"{name} DIED");

        // sumar puntos (si existe GameManager)
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreOnDeath);

        // intentar reproducir animación de muerte si existe
        Animator a = GetComponentInChildren<Animator>();
        if (a != null && !string.IsNullOrEmpty(deathTrigger))
            a.SetTrigger(deathTrigger);

        // desactivar colisiones y lógica de movimiento inmediatamente
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = false;

        var chase = GetComponent<EnemyChase>();
        if (chase != null) chase.NotifyDead();

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // destruir el gameobject tras un pequeño delay (por la animación, si hay)
        Destroy(gameObject, destroyDelay);
    }
}
