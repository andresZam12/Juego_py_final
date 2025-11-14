using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("DISPARO")]
    public float fireRate = 0.2f;
    public int damagePerShot = 10;

    [Header("SONIDO")]
    public AudioClip shootSound;
    public AudioSource audioSource;

    [Header("EFECTOS VISUALES")]
    public ParticleSystem muzzleFlash; // Efecto de fuego en el cañón
    public GameObject impactEffect; // Opcional: efecto al impactar una bala

    [Header("PUNTO DE MIRA")]
    public Transform crosshair; // Referencia al punto rojo (puede ser un UI Image o un objeto en el mundo)
    public Camera playerCamera; // Cámara desde la que se dispara

    [Header("LAYER MASK")]
    public LayerMask enemyLayerMask = 1; // Capa para detectar enemigos

    private float nextFireTime = 0f;

    void Start()
    {
        // Si no se asignó una cámara, usar la cámara principal
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        // Dispara con clic izquierdo (botón 0)
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        Debug.Log("🔫 DISPARO");

        // Sonido de disparo
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Partículas del cañón
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Disparar hacia el punto rojo
        ShootTowardsCrosshair();
    }

    void ShootTowardsCrosshair()
    {
        if (playerCamera == null) return;

        Vector3 shootDirection;
        
        if (crosshair != null)
        {
            // Si el punto rojo es un objeto en el mundo 3D
            shootDirection = (crosshair.position - transform.position).normalized;
        }
        else
        {
            // Si el punto rojo es UI, usar el centro de la pantalla
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            shootDirection = ray.direction;
        }

        // Realizar el raycast
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, shootDirection, out hit, 100f, enemyLayerMask))
        {
            Debug.Log("Impacto en: " + hit.collider.name + " en posición: " + hit.point);
            
            // Verificar si el objeto impactado es un enemigo (MutantEnemy o WarrokEnemy)
            CheckEnemyHit(hit.collider.gameObject);
            
            // Verificar si el objeto impactado es un barril
            CheckBarrelHit(hit.collider.gameObject);
            
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            // Dibujar línea de debug para ver la dirección del disparo (solo en el editor)
            Debug.DrawRay(playerCamera.transform.position, shootDirection * 100f, Color.red, 1f);
        }
    }

    void CheckEnemyHit(GameObject hitObject)
    {
        // PRIMERO: Buscar WarrokEnemy
        WarrokEnemy warrokEnemy = hitObject.GetComponent<WarrokEnemy>();
        if (warrokEnemy == null)
        {
            warrokEnemy = hitObject.GetComponentInParent<WarrokEnemy>();
        }

        if (warrokEnemy != null && !warrokEnemy.IsDead())
        {
            Debug.Log("🎯 WARROK ENEMY IMPACTADO - Aplicando " + damagePerShot + " de daño");
            
            // Aplicar daño al WarrokEnemy
            warrokEnemy.TakeDamage(damagePerShot);
            
            // Efecto visual específico para impacto en enemigo
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hitObject.transform.position + Vector3.up, Quaternion.identity);
            }
            return; // Salir si ya se encontró y dañó un WarrokEnemy
        }

        // SEGUNDO: Buscar MutantEnemy (para compatibilidad con enemigos existentes)
        MutantEnemy mutantEnemy = hitObject.GetComponent<MutantEnemy>();
        if (mutantEnemy == null)
        {
            mutantEnemy = hitObject.GetComponentInParent<MutantEnemy>();
        }

        if (mutantEnemy != null)
        {
            Debug.Log("🎯 MUTANT ENEMY IMPACTADO - Aplicando " + damagePerShot + " de daño");
            
            // Aplicar daño al MutantEnemy
            mutantEnemy.RecibirDano(damagePerShot);
            
            // Efecto visual específico para impacto en enemigo
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hitObject.transform.position + Vector3.up, Quaternion.identity);
            }
        }
    }

    // MÉTODO PARA BARRILES EXPLOSIVOS
    void CheckBarrelHit(GameObject hitObject)
    {
        // Buscar el componente ExplosiveBarrel en el objeto impactado o en sus padres
        ExplosiveBarrel barrel = hitObject.GetComponent<ExplosiveBarrel>();
        
        if (barrel == null)
        {
            // Si no se encuentra en el objeto directo, buscar en los padres
            barrel = hitObject.GetComponentInParent<ExplosiveBarrel>();
        }

        if (barrel != null)
        {
            Debug.Log("🎯 BARRIL IMPACTADO - Aplicando " + damagePerShot + " de daño");
            
            // Aplicar daño al barril
            barrel.TakeDamage(damagePerShot);
            
            // Efecto visual específico para impacto en barril
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hitObject.transform.position, Quaternion.identity);
            }
        }
    }

    // Método para configurar la layer mask desde el inspector fácilmente
    void OnValidate()
    {
        // Esto ayuda a seleccionar layers en el inspector
        if (enemyLayerMask == 0)
            enemyLayerMask = 1; // Default layer
    }
}