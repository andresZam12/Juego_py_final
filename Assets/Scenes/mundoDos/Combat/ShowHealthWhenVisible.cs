using UnityEngine;

public class ShowHealthWhenVisible : MonoBehaviour
{
    [Header("Refs")]
    public Canvas healthCanvas;       // Canvas del enemigo (el que tiene el Slider)
    public Camera playerCamera;       // Arrastra la cámara del jugador

    [Header("Parámetros")]
    public float viewAngle = 35f;     // Ángulo dentro del cual se muestra la barra
    public float maxDistance = 40f;   // Máxima distancia para mostrar

    Transform camT;

    void Awake()
    {
        if (healthCanvas != null) healthCanvas.enabled = false;
    }

    void Start()
    {
        if (playerCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null) playerCamera = cam;
        }
        if (playerCamera != null) camT = playerCamera.transform;
    }

    void LateUpdate()
    {
        if (healthCanvas == null || camT == null) return;

        Vector3 toEnemy = (transform.position - camT.position).normalized;
        float angle = Vector3.Angle(camT.forward, toEnemy);
        float distance = Vector3.Distance(camT.position, transform.position);

        bool canSee = angle <= viewAngle && distance <= maxDistance;

        // Activa/oculta el Canvas
        healthCanvas.enabled = canSee;

        if (canSee)
        {
            // Hacer que la barra mire a la cámara
            healthCanvas.transform.LookAt(
                healthCanvas.transform.position + camT.rotation * Vector3.forward,
                camT.rotation * Vector3.up
            );
        }
    }
}
