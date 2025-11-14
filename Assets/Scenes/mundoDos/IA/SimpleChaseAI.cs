using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleChaseAI : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;          // Arrastra PlayerFPS

    [Header("Persecución")]
    [SerializeField] private float detectRange = 25f;   // Empieza a perseguir
    [SerializeField] private float stopRange = 1.8f;  // Se detiene cerca
    [SerializeField] private float turnSpeed = 8f;    // Suavidad del giro

    [Header("Visual (solo malla)")]
    [SerializeField] private Transform model;           // Arrastra el hijo "Model"
    [SerializeField] private float modelYawOffset = 0f; // 0 / 90 / -90 / 180 según tu FBX

    private NavMeshAgent agent;

    // (Opcional) propiedades públicas por si quieres asignar por código
    public Transform Target { get => target; set => target = value; }
    public Transform Model { get => model; set => model = value; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // Rotamos nosotros el modelo (no el agente)
        agent.updateRotation = false;
    }

    void Update()
    {
        if (!target) return;

        float dist = Vector3.Distance(transform.position, target.position);

        // Moverse si está en rango y aún lejos
        if (dist <= detectRange && dist > stopRange)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
        else
        {
            agent.isStopped = true;
        }

        // Dirección de avance (o hacia el jugador si está parado)
        Vector3 dir = agent.desiredVelocity.sqrMagnitude > 0.01f
                        ? agent.desiredVelocity.normalized
                        : (target.position - transform.position).normalized;

        dir.y = 0f;

        // Girar SOLO la malla visual
        if (model != null && dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up)
                              * Quaternion.Euler(0f, modelYawOffset, 0f);
            model.rotation = Quaternion.Slerp(model.rotation, look, Time.deltaTime * turnSpeed);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, stopRange);
    }
#endif
}
