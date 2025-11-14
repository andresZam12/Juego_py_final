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
    private Vector3 lastPos;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (cc == null)
            Debug.LogError("FPSPlayerController: CharacterController missing!");

        if (cameraHolder == null && transform.childCount > 0)
            cameraHolder = transform.GetChild(0);

        Debug.Log($"PLAYER START - cameraHolder={(cameraHolder!=null ? cameraHolder.name : "null")} cc={(cc!=null)}");
        lastPos = transform.position;

        // lock cursor only when playing in editor/game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // quick guard
        if (cc == null) return;

        // If game window is not focused, log it once (helps debugging)
        if (!Application.isFocused)
        {
            // do not spam, but log once per frame if not focused
            Debug.Log("NOTE: Application not focused. Click Game view to ensure input is captured.");
        }

        // --- debug inputs (temporary)
        float dbg_h = Input.GetAxis("Horizontal");
        float dbg_v = Input.GetAxis("Vertical");
        float dbg_mx = Input.GetAxis("Mouse X");
        if (Mathf.Abs(dbg_h) > 0.01f || Mathf.Abs(dbg_v) > 0.01f || Mathf.Abs(dbg_mx) > 0.01f)
        {
            Debug.Log($"INPUTS -> H:{dbg_h:F2} V:{dbg_v:F2} MX:{dbg_mx:F2}");
        }

        // Movement (read inputs)
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        float h = dbg_h;
        float v = dbg_v;
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

        // Quick sanity: if position didn't change and there were inputs, log that
        if ((Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f) && (Vector3.Distance(transform.position, lastPos) < 0.0001f))
        {
            Debug.LogWarning("Inputs detected but Player position did not change. Check CharacterController or collisions.");
        }
        lastPos = transform.position;
    }
}
