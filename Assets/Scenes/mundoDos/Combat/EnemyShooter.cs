using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Referencias")]
    public Transform target;        // PlayerFPS
    public Transform yawPivot;      // Parte que rota de la torreta
    public Transform firePoint;     // Punto desde donde sale el disparo

    [Header("Detección / Disparo")]
    public float detectRange = 25f;
    public float fireRate = 1.5f;
    public int damage = 10;
    public float maxShotDistance = 60f;
    public float aimHeightOffset = 1.0f; // altura extra para apuntar al pecho/cabeza
    public LayerMask targetMask = ~0;    // Everything de momento

    [Header("FX")]
    public ParticleSystem muzzleFlash;
    public AudioSource shootAudio;

    private float nextShootTime;

    private void Update()
    {
        if (target == null) return;

        // 1) Comprobar si el jugador está dentro del rango
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > detectRange) return;

        // 2) Rotar hacia el jugador (en horizontal)
        if (yawPivot != null)
        {
            Vector3 lookPos = target.position - yawPivot.position;
            lookPos.y = 0f;

            if (lookPos != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookPos);
                yawPivot.rotation = Quaternion.Slerp(
                    yawPivot.rotation,
                    targetRot,
                    Time.deltaTime * 5f
                );
            }
        }

        // 3) Control de cadencia
        if (Time.time >= nextShootTime)
        {
            nextShootTime = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        Debug.Log("Enemigo dispara");

        // FX
        if (muzzleFlash != null) muzzleFlash.Play();
        if (shootAudio != null) shootAudio.Play();

        if (firePoint == null)
        {
            Debug.LogWarning("EnemyShooter: firePoint no asignado");
            return;
        }

        // 4) Calcular dirección exacta hacia el jugador (con pequeña altura extra)
        Vector3 targetPoint = target.position + Vector3.up * aimHeightOffset;
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        Ray ray = new Ray(firePoint.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxShotDistance, targetMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Raycast impacta con: " + hit.collider.name);

            // ⬇⬇⬇ AQUÍ el cambio importante
            SaludDelJugador player = hit.collider.GetComponentInParent<SaludDelJugador>();
            if (player != null)
            {
                Debug.Log("Aplicando daño al jugador");
                player.TakeDamage(damage);
            }
            else
            {
                Debug.Log("El objeto golpeado no tiene SaludDelJugador");
            }
        }
        else
        {
            Debug.Log("Raycast no impacta a nada");
        }

        // Línea roja para ver el rayo en la Scene
        Debug.DrawRay(firePoint.position, direction * maxShotDistance, Color.red, 0.1f);
    }
}
