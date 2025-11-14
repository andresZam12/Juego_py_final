using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("EXPLOSION SETTINGS")]
    public float explosionRadius = 8f;
    public float explosionForce = 1000f;
    public int explosionDamage = 80;
    
    [Header("EFFECTS")]
    public AudioClip explosionSound;
    public GameObject explosionEffect;
    
    private bool isDestroyed = false;

    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;
        Explode();
    }

    void Explode()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        
        Debug.Log("💥 BARRIL EXPLOTADO!");
        
        // Sonido de explosión
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 1f);
        }
        
        // Efecto visual
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // Aplicar fuerza de explosión
        ApplyExplosionForce();
        
        // Destruir el barril
        Destroy(gameObject);
    }

    void ApplyExplosionForce()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hit in colliders)
        {
            // Fuerza física
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 2f);
            }
            
            // Daño a enemigos
            MutantEnemy enemy = hit.GetComponent<MutantEnemy>();
            if (enemy != null && !enemy.EstaMuerto())
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                int calculatedDamage = Mathf.RoundToInt(explosionDamage * damageMultiplier);
                
                enemy.RecibirDano(calculatedDamage);
            }
            
            // Efecto en cadena con otros barriles
            ExplosiveBarrel otherBarrel = hit.GetComponent<ExplosiveBarrel>();
            if (otherBarrel != null && otherBarrel != this)
            {
                otherBarrel.TakeDamage(explosionDamage);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}