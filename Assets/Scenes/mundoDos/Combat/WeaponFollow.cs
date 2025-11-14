using UnityEngine;

public class WeaponFollow : MonoBehaviour
{
    public Transform cam;
    public Vector3 localOffset = new Vector3(0.25f, -0.3f, 0.6f);
    public Vector3 localEuler = Vector3.zero;

    void LateUpdate()
    {
        if (!cam) return;
        transform.position = cam.TransformPoint(localOffset);
        transform.rotation = cam.rotation * Quaternion.Euler(localEuler);
    }
}
