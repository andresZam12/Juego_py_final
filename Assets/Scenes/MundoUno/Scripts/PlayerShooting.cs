using UnityEngine;

// Este script permite al jugador disparar con raycast, aplicar daño a enemigos y reproducir efectos/sonidos
public class PlayerShooting : MonoBehaviour
{
    [Header("DISPARO")]
    public float fireRate = 0.2f; // Tiempo entre disparos
    public int damagePerShot = 20; // Daño por disparo

    [Header("SONIDO")]
    public AudioClip shootSound; // Sonido de disparo
    public AudioSource audioSource; // Fuente de audio para reproducir el sonido

    [Header("EFECTOS VISUALES")]
    public ParticleSystem muzzleFlash; // Efecto de fuego en el cañón
    public GameObject impactEffect; // Efecto al impactar una bala

    [Header("PUNTO DE MIRA")]
    public Transform crosshair; // Referencia al punto rojo (opcional)
    public Camera playerCamera; // Cámara desde la que se dispara

    [Header("LAYER MASK")]
    public LayerMask enemyLayerMask = 1; // Capa para detectar enemigos

    private float nextFireTime = 0f; // Controla el tiempo entre disparos

    void Start()
    {
        // Si no se asignó una cámara, usar la cámara principal
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        // Detecta en
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot(); // Ejecuta el disparo
            nextFireTime = Time.time + fireRate; // Actualiza el tiempo para el próximo disparo
        }
    }

    void Shoot()
    {
        // Reproduce el sonido de disparo
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Efecto cañon
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Lanza el raycast hacia el objetivo
        ShootTowardsCrosshair();
    }

    void ShootTowardsCrosshair()
    {
        if (playerCamera == null) return;

        Vector3 shootDirection;
        // Calcula la dirección del disparo según el punto de mira
        if (crosshair != null)
        {
            shootDirection = (crosshair.position - transform.position).normalized;
        }
        else
        {
            // Si no hay crosshair, dispara al centro de la pantalla
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            shootDirection = ray.direction;
        }

        // Realiza el raycast para detectar impacto
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, shootDirection, out hit, 100f, enemyLayerMask))
        {
            // Si impacta algo, verifica si es enemigo o barril
            CheckEnemyHit(hit.collider.gameObject);
            CheckBarrelHit(hit.collider.gameObject);
            // Instancia efecto visual de impacto
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }

    // Verifica si el objeto impactado es un enemigo y aplica daño
    void CheckEnemyHit(GameObject hitObject)
    {
        // Busca WarrokEnemy
        WarrokEnemy warrokEnemy = hitObject.GetComponent<WarrokEnemy>();
        if (warrokEnemy == null)
            warrokEnemy = hitObject.GetComponentInParent<WarrokEnemy>();

        if (warrokEnemy != null && !warrokEnemy.IsDead())
        {
            warrokEnemy.TakeDamage(damagePerShot);
            if (impactEffect != null)
                Instantiate(impactEffect, hitObject.transform.position + Vector3.up, Quaternion.identity);
            return;
        }

        // Busca MutantEnemy
        MutantEnemy mutantEnemy = hitObject.GetComponent<MutantEnemy>();
        if (mutantEnemy == null)
            mutantEnemy = hitObject.GetComponentInParent<MutantEnemy>();

        if (mutantEnemy != null && !mutantEnemy.EstaMuerto())
        {
            mutantEnemy.RecibirDano(damagePerShot);
            if (impactEffect != null)
                Instantiate(impactEffect, hitObject.transform.position + Vector3.up, Quaternion.identity);
        }
    }

    // Verifica si el objeto impactado es un barril explosivo y aplica daño
    void CheckBarrelHit(GameObject hitObject)
    {
        ExplosiveBarrel barrel = hitObject.GetComponent<ExplosiveBarrel>();
        if (barrel == null)
            barrel = hitObject.GetComponentInParent<ExplosiveBarrel>();

        if (barrel != null)
        {
            barrel.TakeDamage(damagePerShot);
            if (impactEffect != null)
                Instantiate(impactEffect, hitObject.transform.position, Quaternion.identity);
        }
    }

    // Método para facilitar la selección de layer en el inspector
    void OnValidate()
    {
        if (enemyLayerMask == 0)
            enemyLayerMask = 1; // Default layer
    }
}