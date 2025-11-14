using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public CharacterController controller;

    [Header("Movimiento")]
    public float playerSpeed = 7f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    void Awake()
    {
        // Si no arrastras nada en el inspector,
        // busca el CharacterController en el mismo objeto.
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Comprobar si está en el suelo
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // pegado al suelo

        // Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * playerSpeed * Time.deltaTime);

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
