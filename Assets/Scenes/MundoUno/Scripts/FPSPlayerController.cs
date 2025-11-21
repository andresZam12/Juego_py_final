using UnityEngine;

// Este script controla el movimiento y la rotación del jugador en primera persona (FPS)
// Permite caminar, correr, saltar, aplicar gravedad y mirar con el mouse
[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    // Velocidad de caminar
    public float walkSpeed = 5f;
    // Velocidad de correr (Shift)
    public float runSpeed = 8f;
    // Velocidad de salto
    public float jumpSpeed = 7f;
    // Valor de gravedad (negativo)
    public float gravity = -9.81f;
    // Sensibilidad del mouse para mirar
    public float mouseSensitivity = 2f;
    // Referencia al objeto hijo que contiene la cámara
    public Transform cameraHolder; // asignar el hijo Camera aquí
    // Ángulo máximo de mirada vertical (para evitar girar completamente)
    public float maxLookAngle = 80f;

    // Referencia al CharacterController
    private CharacterController cc;
    // Velocidad vertical acumulada (para saltos y gravedad)
    private float verticalVel;
    // Ángulo de inclinación vertical de la cámara
    private float pitch = 0f;

    void Start()
    {
        // Obtiene el CharacterController
        cc = GetComponent<CharacterController>();
        if (cc == null)
            Debug.LogError("FPSPlayerController: CharacterController missing!");

        // Si no se asignó cameraHolder, toma el primer hijo
        if (cameraHolder == null && transform.childCount > 0)
            cameraHolder = transform.GetChild(0);

        // Bloquea y oculta el cursor para modo FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Si no hay CharacterController, no hace nada
        if (cc == null) return;

        // --- MOVIMIENTO HORIZONTAL ---
        // Si se mantiene Shift, corre; si no, camina
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        // Lee input de teclado (A/D y W/S)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        // Calcula dirección de movimiento en el plano XZ
        Vector3 moveDir = transform.right * h + transform.forward * v;

        // --- SALTO Y GRAVEDAD ---
        if (cc.isGrounded)
        {
            // Si está en el suelo y se presiona espacio, salta
            if (Input.GetButtonDown("Jump"))
                verticalVel = jumpSpeed;
            else
                verticalVel = -1f; // Pequeña fuerza hacia abajo para mantener pegado al suelo
        }
        else
        {
            // Si está en el aire, aplica gravedad acumulativa
            verticalVel += gravity * Time.deltaTime;
        }

        // Combina movimiento horizontal con velocidad vertical
        Vector3 velocity = moveDir.normalized * speed;
        velocity.y = verticalVel;

        // Mueve el personaje usando CharacterController
        cc.Move(velocity * Time.deltaTime);

        // --- ROTACIÓN CON MOUSE ---
        // Lee movimiento del mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        // Rota el jugador horizontalmente (eje Y)
        transform.Rotate(Vector3.up * mouseX);

        // Calcula y limita la rotación vertical (pitch)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        // Aplica la rotación vertical solo a la cámara
        if (cameraHolder != null)
            cameraHolder.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
