using UnityEngine;
using UnityEngine.AI;

public class MutantEnemy : MonoBehaviour
{
    [Header("OBJETIVO")]
    public Transform objetivo;
    
    [Header("VELOCIDAD")]
    public float velocidadMovimiento = 3f;
    public float velocidadRotacion = 5f;
    public bool rotacionSuave = true;
    
    [Header("IA - NAVEGACIÓN")]
    public NavMeshAgent agenteIA;
    
    [Header("AUDIO - ATAQUE")]
    public AudioClip sonidoAtaque;
    public AudioSource audioSourceAtaque;
    
    [Header("AUDIO - MUERTE")]
    public AudioClip sonidoMuerte;
    public AudioSource audioSourceMuerte;
    
    [Header("SALUD")]
    public float saludMaxima = 100f;
    public float saludActual = 100f;
    
    [Header("ATAQUE")]
    public int dañoAtaque = 10;
    public float rangoAtaque = 2f;
    public float cooldownAtaque = 2f;
    
    [Header("DETECCIÓN")]
    public float rangoDeteccion = 30f;
    public float rangoPerdida = 40f;
    
    // Propiedades públicas para la barra de vida
    public float health => saludActual;
    public float maxHealth => saludMaxima;
    
    private Animator animator;
    private float tiempoUltimoAtaque;
    private bool estaVivo = true;
    private bool estaAtacando = false;
    
    private enum EstadoEnemigo
    {
        Idle,
        Run,
        Attack,
        Death
    }
    private EstadoEnemigo estadoActual = EstadoEnemigo.Idle;

    void Start()
    {
        InicializarComponentes();
        ConfigurarAgenteIA();
        saludActual = saludMaxima;
        ConfigurarAnimacionesIniciales();
    }

    void InicializarComponentes()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (agenteIA == null)
            agenteIA = GetComponent<NavMeshAgent>();
            
        if (objetivo == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
                objetivo = jugador.transform;
            else
                Debug.LogError("No se encontró el jugador con tag 'Player'");
        }
    }

    void ConfigurarAnimacionesIniciales()
    {
        if (animator != null)
        {
            animator.SetBool("IsIdle", true);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsDead", false);
        }
    }

    void ConfigurarAgenteIA()
    {
        if (agenteIA != null)
        {
            agenteIA.speed = velocidadMovimiento;
            agenteIA.angularSpeed = velocidadRotacion * 100f;
            agenteIA.stoppingDistance = rangoAtaque - 0.2f;
        }
    }

    void Update()
    {
        if (!estaVivo) return;
        
        VerificarEstadoSalud();
        MaquinaDeEstados();
        ActualizarRotacion();
        ActualizarAnimaciones();
    }

    void VerificarEstadoSalud()
    {
        if (saludActual <= 0 && estaVivo)
        {
            Morir();
        }
    }

    void MaquinaDeEstados()
    {
        if (objetivo == null) return;

        switch (estadoActual)
        {
            case EstadoEnemigo.Idle:
                EstadoIdle();
                break;
                
            case EstadoEnemigo.Run:
                EstadoRun();
                break;
                
            case EstadoEnemigo.Attack:
                EstadoAttack();
                break;
        }
    }

    void EstadoIdle()
    {
        if (ObjetivoEnRango(rangoDeteccion) && objetivo != null)
        {
            CambiarEstado(EstadoEnemigo.Run);
        }
    }

    void EstadoRun()
    {
        if (objetivo == null)
        {
            CambiarEstado(EstadoEnemigo.Idle);
            return;
        }
        
        if (agenteIA != null && agenteIA.isActiveAndEnabled)
        {
            agenteIA.SetDestination(objetivo.position);
        }
        
        // VERIFICAR SI PUEDE ATACAR
        if (ObjetivoEnRango(rangoAtaque))
        {
            CambiarEstado(EstadoEnemigo.Attack);
        }
        
        if (!ObjetivoEnRango(rangoPerdida))
        {
            CambiarEstado(EstadoEnemigo.Idle);
        }
    }

    void EstadoAttack()
    {
        if (objetivo == null)
        {
            CambiarEstado(EstadoEnemigo.Idle);
            return;
        }

        // DETENER EL MOVIMIENTO
        if (agenteIA != null && agenteIA.hasPath)
        {
            agenteIA.isStopped = true;
        }
        
        // ROTAR HACIA EL OBJETIVO
        RotarHaciaObjetivo();
        
        // VERIFICAR SI PUEDE ATACAR
        bool puedeAtacar = Time.time - tiempoUltimoAtaque >= cooldownAtaque;
        bool enRango = ObjetivoEnRango(rangoAtaque);
        
        if (puedeAtacar && enRango && !estaAtacando)
        {
            IniciarAtaque();
        }
        
        // SI SE ALEJÓ DEMASIADO, PERSEGUIR
        if (!enRango && !estaAtacando)
        {
            CambiarEstado(EstadoEnemigo.Run);
        }
    }

    void RotarHaciaObjetivo()
    {
        if (objetivo == null) return;
        
        Vector3 direccion = (objetivo.position - transform.position).normalized;
        direccion.y = 0;
        
        if (direccion != Vector3.zero)
        {
            if (rotacionSuave)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(direccion);
            }
        }
    }

    void ActualizarRotacion()
    {
        if (agenteIA != null && agenteIA.hasPath && estadoActual == EstadoEnemigo.Run && !agenteIA.isStopped)
        {
            if (rotacionSuave && agenteIA.velocity.magnitude > 0.1f)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(agenteIA.velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
            }
        }
    }

    void ActualizarAnimaciones()
    {
        if (animator == null) return;

        // Actualizar velocidad para la animación de caminar/correr
        float velocidad = 0f;
        if (agenteIA != null && estadoActual == EstadoEnemigo.Run)
        {
            velocidad = agenteIA.velocity.magnitude;
        }
        animator.SetFloat("Speed", velocidad);
        
        // Actualizar estado de ataque
        animator.SetBool("IsAttacking", estaAtacando);
    }

    void IniciarAtaque()
    {
        estaAtacando = true;
        tiempoUltimoAtaque = Time.time;
        
        if (animator != null)
        {
            // USAR TRIGGER PARA LA ANIMACIÓN DE ATAQUE
            animator.SetTrigger("Attack");
            animator.SetBool("IsAttacking", true);
        }
        
        if (sonidoAtaque != null && audioSourceAtaque != null)
        {
            audioSourceAtaque.clip = sonidoAtaque;
            audioSourceAtaque.Play();
        }
        
        // PROGRAMAR EL DAÑO (ajusta el tiempo según tu animación)
        Invoke("AplicarDaño", 0.5f);
        
        // TERMINAR EL ATAQUE (ajusta el tiempo según la duración de tu animación)
        Invoke("TerminarAtaque", 1.5f);
    }

    void AplicarDaño()
    {
        if (!estaVivo || !estaAtacando) return;
        
        if (ObjetivoEnRango(rangoAtaque) && objetivo != null)
        {
            PlayerHealthMundoUno playerHealth = objetivo.GetComponent<PlayerHealthMundoUno>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(dañoAtaque);
            }
        }
    }

    void TerminarAtaque()
    {
        estaAtacando = false;
        
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
        
        // REANUDAR MOVIMIENTO SI ES NECESARIO
        if (estadoActual == EstadoEnemigo.Attack && agenteIA != null)
        {
            if (!ObjetivoEnRango(rangoAtaque))
            {
                CambiarEstado(EstadoEnemigo.Run);
            }
        }
    }

    void Morir()
    {
        estaVivo = false;
        CambiarEstado(EstadoEnemigo.Death);
        
        // Sumar score al GameManager
        if (GameManager.Instance != null)
        {
            int scoreAmount = 50; // Puntos por matar este enemigo
            GameManager.Instance.AddScore(scoreAmount);
            Debug.Log($"💰 Enemigo muerto! +{scoreAmount} puntos");
        }
        
        if (sonidoMuerte != null && audioSourceMuerte != null)
        {
            audioSourceMuerte.clip = sonidoMuerte;
            audioSourceMuerte.Play();
        }
        
        if (agenteIA != null)
        {
            agenteIA.isStopped = true;
            agenteIA.enabled = false;
        }
        
        // CANCELAR CUALQUIER INVOCACIÓN PENDIENTE
        estaAtacando = false;
        CancelInvoke("AplicarDaño");
        CancelInvoke("TerminarAtaque");
        
        Destroy(gameObject, 0.1f); // Desaparece más rápido
    }

    public void RecibirDano(int dano)
    {
        if (!estaVivo) return;
        
        saludActual -= dano;
        saludActual = Mathf.Clamp(saludActual, 0, saludMaxima);
    }

    public bool EstaMuerto()
    {
        return !estaVivo;
    }

    bool ObjetivoEnRango(float rango)
    {
        if (objetivo == null) return false;
        
        float distancia = Vector3.Distance(transform.position, objetivo.position);
        return distancia <= rango;
    }

    void CambiarEstado(EstadoEnemigo nuevoEstado)
    {
        if (estadoActual == nuevoEstado) return;
        
        estadoActual = nuevoEstado;
        
        // CONFIGURAR AGENTE DE NAVEGACIÓN
        if (agenteIA != null)
        {
            switch (estadoActual)
            {
                case EstadoEnemigo.Idle:
                    agenteIA.isStopped = true;
                    agenteIA.ResetPath();
                    break;
                    
                case EstadoEnemigo.Run:
                    agenteIA.isStopped = false;
                    break;
                    
                case EstadoEnemigo.Attack:
                    agenteIA.isStopped = true;
                    break;
                    
                case EstadoEnemigo.Death:
                    agenteIA.isStopped = true;
                    break;
            }
        }
        
        // CONFIGURAR ANIMACIONES
        if (animator != null)
        {
            animator.SetBool("IsIdle", estadoActual == EstadoEnemigo.Idle);
            animator.SetBool("IsRunning", estadoActual == EstadoEnemigo.Run);
            animator.SetBool("IsDead", estadoActual == EstadoEnemigo.Death);
        }
    }

    // Gizmos para visualizar los rangos en el editor
    void OnDrawGizmosSelected()
    {
        // Rango de ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        
        // Rango de pérdida (azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangoPerdida);
        
        // Línea hacia el objetivo (verde si en rango, rojo si no)
        if (objetivo != null)
        {
            float distancia = Vector3.Distance(transform.position, objetivo.position);
            Gizmos.color = distancia <= rangoAtaque ? Color.green : Color.white;
            Gizmos.DrawLine(transform.position, objetivo.position);
        }
    }
}