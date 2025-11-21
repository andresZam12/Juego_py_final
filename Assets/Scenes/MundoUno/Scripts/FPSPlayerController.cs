using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpSpeed = 7f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 2f;
    public Transform cameraHolder; // assign the child Camera transform here
    public float maxLookAngle = 80f;

    private CharacterController cc;
    private float verticalVel;
    private float pitch = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (cc == null)
            Debug.LogError("FPSPlayerController: CharacterController missing!");

        if (cameraHolder == null && transform.childCount > 0)
            cameraHolder = transform.GetChild(0);

        // lock cursor only when playing in editor/game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // quick guard
        if (cc == null) return;

        // Movement (read inputs)
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveDir = transform.right * h + transform.forward * v;

        // Jump / Gravity
        if (cc.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
                verticalVel = jumpSpeed;
            else
                verticalVel = -1f; // small downward to keep grounded
        }
        else
        {
            verticalVel += gravity * Time.deltaTime;
        }

        // Combine horizontal/vertical movement with vertical velocity
        Vector3 velocity = moveDir.normalized * speed;
        velocity.y = verticalVel;

        // Move character
        cc.Move(velocity * Time.deltaTime);

        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        if (cameraHolder != null)
            cameraHolder.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
