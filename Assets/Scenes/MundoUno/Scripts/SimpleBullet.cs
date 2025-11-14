using UnityEngine;

public class SimpleBullet : MonoBehaviour
{
    public int damage = 25;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var eh = collision.collider.GetComponentInParent<EnemyHealth>();
        if (eh != null)
        {
            eh.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
