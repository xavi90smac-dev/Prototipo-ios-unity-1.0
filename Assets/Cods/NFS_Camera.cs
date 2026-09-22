using UnityEngine;

public class NFS_Camera : MonoBehaviour
{
    [Header("=== OBJETIVO ===")]
    public Transform target;

    [Header("=== CÁMARA NORMAL (CONDUCCIÓN RECTA) ===")]
    public float normalDistance = 3.0f;  
    public float normalHeight = 0.8f;    

    [Header("=== CONFIGURACIÓN DE DERRIPE (DRIFT) ===")]
    public float driftDistance = 3.6f;     
    public float driftHeight = 0.9f;       
    public float driftLookAtHeight = 0.4f;
    public float driftRotationSway = 10f;

    [Header("=== SUAVIZADO (ANIMACIÓN) ===")]
    public float positionSmooth = 6f;      
    public float rotationSmooth = 10f;      

    private ArcadeCarController carController;
    private float smoothSteer = 0f;

    void Start()
    {
        if (target != null)
        {
            carController = target.GetComponent<ArcadeCarController>();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float driftFactor = 0f;
        float steerVal = 0f;

        if (carController != null)
        {
            driftFactor = carController.driftIntensity; 
            steerVal = carController.currentSteer;      
        }

        smoothSteer = Mathf.Lerp(smoothSteer, steerVal, 4f * Time.deltaTime);

        // Interpolación limpia entre distancia normal y de drift
        float currentDist = Mathf.Lerp(normalDistance, driftDistance, driftFactor);
        float currentHeight = Mathf.Lerp(normalHeight, driftHeight, driftFactor);
        
        Vector3 targetPosition = target.position 
            - (target.forward * currentDist) 
            + (Vector3.up * currentHeight);

        // Suavizado dinámico: si estamos saliendo del drift, hacemos la transición de posición más lenta para que no pegue saltos
        float currentPosSmooth = Mathf.Lerp(positionSmooth, positionSmooth * 0.6f, driftFactor);
        transform.position = Vector3.Lerp(transform.position, targetPosition, currentPosSmooth * Time.deltaTime);

        // Punto de mira y rotación
        float currentLookHeight = Mathf.Lerp(0.4f, driftLookAtHeight, driftFactor);
        Vector3 lookAtPoint = target.position + (Vector3.up * currentLookHeight);
        Quaternion baseRotation = Quaternion.LookRotation(lookAtPoint - transform.position);

        float yawOffset = smoothSteer * driftRotationSway * driftFactor; 
        Quaternion finalRotation = baseRotation * Quaternion.Euler(0f, yawOffset, 0f);

        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, rotationSmooth * Time.deltaTime);
    }
}