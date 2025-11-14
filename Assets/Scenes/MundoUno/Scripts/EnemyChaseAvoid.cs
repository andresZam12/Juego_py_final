using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEngine.AI;
#endif

[RequireComponent(typeof(Collider))]
public class EnemyChaseAvoid : MonoBehaviour
{
    [Header("Target / Movement")]
    public Transform target;
    public float speed = 2.5f;
    public float stoppingDistance = 1.2f;
    public float turnSpeed = 5f;
    public string walkBool = "Walk";

    [Header("Avoidance / jitter")]
    public float followRadius = 1.0f;        // offset around player to follow
    public float separationRadius = 1.0f;    // how close to other enemies before repelling
    public float separationStrength = 2.0f;  // strength of repulsion
    public float startDelayMin = 0f;         // random start delay
    public float startDelayMax = 0.6f;

    Animator animator;
    bool started = false;
    Vector3 followOffset;
    float startDelay = 0f;

    // NavMeshAgent if available
    UnityEngine.AI.NavMeshAgent agent;
    bool useAgent = false;

    // CharacterController if present (to respect collisions)
    CharacterController cc;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        useAgent = (agent != null);
        cc = GetComponent<CharacterController>();

        startDelay = Random.Range(startDelayMin, startDelayMax);

        // random follow offset so not all targets the exact same point
        followOffset = Random.insideUnitSphere * followRadius;
        followOffset.y = 0f;
    }

    IEnumerator StartDelay()
    {
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);
        started = true;
    }

    void OnEnable()
    {
        started = false;
        StartCoroutine(StartDelay());
    }

    void Update()
    {
        if (!started) return;

        // ensure we have a target
        if (target == null)
        {
            var t = GameObject.FindWithTag("Player");
            if (t != null) target = t.transform;
            if (target == null) return;
        }

        Vector3 desired = target.position + followOffset;

        // We'll compute a "move" vector (horizontal) and a boolean walking for animation
        Vector3 move = Vector3.zero;
        bool walking = false;

        if (useAgent)
        {
            // NavMeshAgent mode
            agent.speed = speed;
            agent.stoppingDistance = stoppingDistance;

            // set destination if it's far enough or no path
            if (!agent.hasPath || agent.destination != desired)
                agent.SetDestination(desired);

            // walking detection: agent.remainingDistance may be invalid while path pending
            if (!agent.pathPending)
            {
                // use a small epsilon to ensure not flipping
                walking = agent.remainingDistance > agent.stoppingDistance + 0.05f;
            }

            // If using CharacterController in combination, we don't move manually here
            // NavMeshAgent will handle motion (unless you want to use agent.nextPosition)
        }
        else
        {
            // Transform-based movement with simple separation
            Vector3 dir = desired - transform.position;
            dir.y = 0f;
            float dist = dir.magnitude;

            if (dist > stoppingDistance)
            {
                move = dir.normalized * speed * Time.deltaTime;
                walking = true;
            }

            // separation: find nearby enemies and push away a bit
            Vector3 separation = Vector3.zero;
            Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius);
            foreach (var c in hits)
            {
                if (c.gameObject == this.gameObject) continue;
                if (c.CompareTag("Enemy"))
                {
                    Vector3 toOther = transform.position - c.transform.position;
                    toOther.y = 0f; // IMPORTANT: ignore vertical difference
                    float d = toOther.magnitude;
                    if (d > 0.001f)
                    {
                        // avoid explosion when d->0 by clamping min distance
                        float contrib = 1.0f / Mathf.Max(d, 0.3f);
                        separation += toOther.normalized * contrib;
                    }
                }
            }
            if (separation != Vector3.zero)
            {
                separation.y = 0f;
                separation = separation.normalized * separationStrength * Time.deltaTime;
                // cap separation magnitude to avoid huge pushes
                float maxSep = separationStrength * 0.5f * Time.deltaTime;
                if (separation.magnitude > maxSep) separation = separation.normalized * maxSep;
                move += separation;
            }
        }

        // APPLY movement: prefer CharacterController if present, else NavMeshAgent for agent, else transform
        if (cc != null)
        {
            // move horizontal only - CharacterController handles collisions
            Vector3 horizontalMove = new Vector3(move.x, 0f, move.z);
            cc.Move(horizontalMove);

            // keep the enemy on the terrain surface (sample terrain height)
            Terrain t = Terrain.activeTerrain;
            if (t != null)
            {
                Vector3 pos = transform.position;
                float terrainY = t.SampleHeight(pos) + t.transform.position.y;
                float desiredY = terrainY - cc.center.y + (cc.height * 0.5f);
                // only snap up if below terrain to avoid popping
                if (pos.y < desiredY - 0.01f)
                {
                    transform.position = new Vector3(pos.x, desiredY, pos.z);
                }
            }

            // rotate smoothly toward movement direction if we moved
            if (horizontalMove.sqrMagnitude > 0.000001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(horizontalMove.x, 0, horizontalMove.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
            }
        }
        else if (useAgent)
        {
            // If agent is present but no CharacterController, let agent do the movement.
            // However, ensure the transform follows the agent's position (if agent updates position)
            // This assumes agent.updatePosition = true (default). If not, you can set transform.position = agent.nextPosition.
            // We'll just make sure rotation follows the movement direction
            if (!agent.pathPending && agent.remainingDistance > 0.01f)
            {
                Vector3 vel = agent.velocity;
                vel.y = 0f;
                if (vel.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(vel.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
                }
            }
        }
        else
        {
            // fallback: move transform directly (we already prepared 'move')
            Vector3 applyMove = new Vector3(move.x, 0f, move.z);
            if (applyMove.sqrMagnitude > 0.000001f)
            {
                transform.position += applyMove;
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(applyMove.x, 0, applyMove.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
            }
        }

        // Update animator safely: only if animator exists and has controller
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(walkBool, walking);
        }
    }

    // Called from EnemyHealth when dying
    public void NotifyDead()
    {
        started = false;
        if (useAgent && agent != null) agent.isStopped = true;
        if (animator != null && animator.runtimeAnimatorController != null) animator.SetBool(walkBool, false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
