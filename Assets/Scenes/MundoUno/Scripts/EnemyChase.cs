using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    public Transform target;
    public float speed = 2.5f;
    public float stoppingDistance = 1.5f;
    public float turnSpeed = 5f;
    public string walkBool = "Walk"; // opcional: nombre del bool en Animator

    Animator animator;
    bool dead = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (target == null)
        {
            var t = GameObject.FindWithTag("Player");
            if (t != null) target = t.transform;
        }
    }

    void Update()
    {
        if (dead) return;
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;
        if (dist > stoppingDistance)
        {
            Vector3 move = dir.normalized * speed * Time.deltaTime;
            transform.position += move;

            // rotación suave hacia el jugador
            if (move.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
            }

            if (animator != null && !string.IsNullOrEmpty(walkBool))
                animator.SetBool(walkBool, true);
        }
        else
        {
            if (animator != null && !string.IsNullOrEmpty(walkBool))
                animator.SetBool(walkBool, false);
            // aquí podrías implementar ataque
        }
    }

    // Llamar desde EnemyHealth cuando muera para cortar movimiento (opcional)
    public void NotifyDead()
    {
        dead = true;
        if (animator != null)
        {
            if (!string.IsNullOrEmpty(walkBool))
                animator.SetBool(walkBool, false);
        }
    }
}
