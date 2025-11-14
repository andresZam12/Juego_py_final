using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab del enemigo (debe tener SaludDelEnemigo)")]
    public SaludDelEnemigo enemyPrefab;

    [Header("Puntos de spawn (si se deja vacío, usa este transform)")]
    public Transform[] spawnPoints;

    [Header("Ajustes de respawn")]
    public bool spawnOnStart = true;
    public float respawnDelay = 3f;

    // referencia al enemigo que está actualmente vivo
    private SaludDelEnemigo current;

    private void Start()
    {
        if (spawnOnStart)
            Spawn();
    }

    public void Spawn()
    {
        // si ya hay enemigo vivo, no spawnear otro
        if (current != null) return;

        Transform p = (spawnPoints != null && spawnPoints.Length > 0)
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : transform;

        current = Instantiate(enemyPrefab, p.position, p.rotation);
        current.OnDeath += HandleDeath;
    }

    private void HandleDeath(SaludDelEnemigo eh)
    {
        if (current != null)
            current.OnDeath -= HandleDeath;

        current = null;
        Invoke(nameof(Spawn), respawnDelay);
    }
}
