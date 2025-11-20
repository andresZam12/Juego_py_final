using UnityEngine;

public class Rifle : MonoBehaviour
{
    public Transform playerCamera;
    public float shotDistance = 10f;
    public float impactForce = 5f;
    public LayerMask shotMask;
    public GameObject destroyEffect;
    public ParticleSystem shootParticles;
    public GameObject hitEffect;
    
    [Header("Audio")]
    public AudioClip hitSound;
    public AudioSource audioSource;
    
    private RaycastHit showRaycastHit;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            Shoot();
    }

    private void Shoot()
    {
        if (shootParticles != null)
            shootParticles.Play();

        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out showRaycastHit, shotDistance, shotMask))
        {
            Debug.Log("Shot hit: " + showRaycastHit.collider.name);

            // Efecto de impacto
            if (hitEffect != null)
                Instantiate(hitEffect, showRaycastHit.point, Quaternion.LookRotation(showRaycastHit.normal));

            // Aplicar fuerza física
            Rigidbody rb = showRaycastHit.collider.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(-showRaycastHit.normal * impactForce, ForceMode.Impulse);

            // Sonido de impacto
            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);

            // Verificar si le disparamos a un ENEMIGO
            SkeletonEnemy enemy = showRaycastHit.collider.GetComponentInParent<SkeletonEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(50f);
                return;
            }

            // Verificar si le disparamos a un BARRIL u objeto destructible
            if (showRaycastHit.collider.CompareTag("Respawn"))
            {
                // Efecto de destrucción
                if (destroyEffect != null)
                    Instantiate(destroyEffect, showRaycastHit.point, Quaternion.LookRotation(showRaycastHit.normal));

                // Destruir el objeto
                Destroy(showRaycastHit.collider.gameObject);
                
                Debug.Log("Barril destruido");
            }
        }
    }
}