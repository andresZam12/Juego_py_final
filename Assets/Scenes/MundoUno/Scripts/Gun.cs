using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("General")]
    public Transform muzzlePoint;
    public Camera cam;
    public bool useRaycast = true;

    [Header("Fire")]
    public float fireRate = 0.2f;
    public int damage = 25;
    public float range = 100f;
    private float nextFireTime = 0f;

    [Header("Projectile settings (if not hitscan)")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 50f;
    [Tooltip("Small offset forward from the muzzlePoint to avoid immediate collisions with the gun model")]
    public float spawnOffset = 0.12f;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject impactPrefab;    // prefab para efecto de impacto (decals/particles)
    public AudioClip shootSound;
    public float recoilPerShot = 4f;        // cuánto añade cada disparo (grados)
    public float recoilRecoverSpeed = 8f;   // cuánto se recupera por segundo
    public float maxRecoil = 30f;          // límite del recoil acumulado

    AudioSource audioSrc;

    // recoil gestionado
    private float recoilCurrent = 0f;       // valor actual acumulado de recoil (grados)

    void Start()
    {
        if (cam == null) cam = Camera.main;
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();

        if (muzzleFlash != null)
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        // Recuperar recoil suavemente cada frame
        recoilCurrent = Mathf.Lerp(recoilCurrent, 0f, Time.deltaTime * recoilRecoverSpeed);

        // disparo (mantener botón) con rate limiting
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    // Aplicar recoil en LateUpdate para que se ejecute DESPUÉS del Player (y respete su pitch)
    void LateUpdate()
    {
        if (cam == null) return;

        // obtener ángulo X actual de la cámara en forma "signada"
        Vector3 camAngles = cam.transform.localEulerAngles;
        float signedX = camAngles.x;
        if (signedX > 180f) signedX -= 360f;

        // restamos el recoil acumulado (así el player pitch - recoil)
        float newX = signedX - recoilCurrent;

        cam.transform.localEulerAngles = new Vector3(newX, camAngles.y, camAngles.z);
    }

    void Fire()
    {
        // efectos
        if (muzzleFlash != null) muzzleFlash.Play();
        if (audioSrc != null && shootSound != null) audioSrc.PlayOneShot(shootSound);

        // aumentar recoil acumulado (se suaviza en Update)
        recoilCurrent += recoilPerShot;
        recoilCurrent = Mathf.Clamp(recoilCurrent, 0f, maxRecoil);

        if (useRaycast)
            DoRaycast();
        else
            SpawnProjectile();
    }

    void DoRaycast()
    {
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log($"Gun: hit {hit.collider.name} at distance {hit.distance}");

            // aplicar daño a enemigo si existe
            var eh = hit.collider.GetComponentInParent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(damage);
            }

            // instanciar efecto de impacto si está asignado
            if (impactPrefab != null)
            {
                // posicionar ligeramente alejando por la normal para evitar z-fighting
                Vector3 pos = hit.point + hit.normal * 0.01f;
                Quaternion rot = Quaternion.LookRotation(hit.normal);
                GameObject impact = Instantiate(impactPrefab, pos, rot);
                // opcional: parentear al objeto impactado para que se mueva con él
                if (hit.collider != null)
                    impact.transform.SetParent(hit.collider.transform, true);
                Destroy(impact, 8f);
            }
        }
    }

    void SpawnProjectile()
    {
        if (bulletPrefab == null || muzzlePoint == null || cam == null) return;

        // calcular posición de spawn (ligeramente adelantada respecto al muzzle)
        Vector3 spawnPos = muzzlePoint.position + muzzlePoint.forward * spawnOffset;
        Quaternion spawnRot = muzzlePoint.rotation;

        GameObject b = Instantiate(bulletPrefab, spawnPos, spawnRot);

        // asegurar que la bala tenga Rigidbody y configuración adecuada
        Rigidbody rb = b.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = b.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // aplicar velocidad usando la forward del muzzle (sale desde la boca del arma)
        rb.velocity = muzzlePoint.forward * bulletSpeed;

        // si el prefab tiene SimpleBullet o similar, dejar que gestione el daño por colisión
        // si quieres, puedes ajustar layer/ignore collisions aquí
    }
}
