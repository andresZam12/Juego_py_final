using UnityEngine;

public class XMouseLookUno : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerTransform;
    float rotX = 0f;

    void Start() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX - mouseY, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotX, 0f, 0f);
        playerTransform.Rotate(Vector3.up * mouseX);

        if (Input.GetKeyDown(KeyCode.Escape)) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}
