using UnityEngine;

public class Arma : MonoBehaviour
{
    [Header("Referencia")]
    public Camera playerCamera;          // Cámara del jugador FPS

    [Header("Stats")]
    public int damage = 1;               // daño por disparo
    public float range = 60f;
    public float fireRate = 10f;
    public float impactForce = 7f;

    [Header("FX")]
    public ParticleSystem muzzleFlash;
    public GameObject hitEffectPrefab;
    public AudioSource shootAudio;

    private float nextTimeToFire = 0f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        Debug.Log("DISPARO DEL JUGADOR");

        // FX: fogonazo
        if (muzzleFlash != null)
            muzzleFlash.Play();

        // FX: sonido del disparo
        if (shootAudio != null && shootAudio.clip != null)
            shootAudio.PlayOneShot(shootAudio.clip);

        if (playerCamera == null)
        {
            Debug.LogError("Gun: playerCamera NO asignada en el Inspector");
            return;
        }

        // Raycast sin máscara por ahora (Everything)
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Raycast impacta con: " + hit.collider.name);

            // FX impacto
            if (hitEffectPrefab != null)
            {
                GameObject fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(fx, 2f);
            }

            // Empuje físico
            if (hit.rigidbody != null)
                hit.rigidbody.AddForce(-hit.normal * impactForce, ForceMode.Impulse);

            // DAÑO AL ENEMIGO
            SaludDelEnemigo enemy = hit.collider.GetComponentInParent<SaludDelEnemigo>();
            if (enemy != null)
            {
                Debug.Log("Disparo impacta ENEMIGO: " + enemy.name);
                enemy.TakeDamage(damage);
            }
            else
            {
                Debug.Log("Disparo NO impacta SaludEnemigo (golpeó algo sin SaludDelEnemigo)");
            }
        }
        else
        {
            Debug.Log("Raycast NO golpeó nada");
        }
    }
}
