using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    public Camera targetCamera;

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            if (Camera.main == null) return;
            targetCamera = Camera.main;
        }

        Vector3 dir = (transform.position - targetCamera.transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
