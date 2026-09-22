using UnityEngine;

public class CinematicCameraMove : MonoBehaviour
{
    [Header("Puntos de Movimiento (Opcional)")]
    [Tooltip("Arrastra un objeto vacío si quieres que esta cámara se mueva. Si se queda vacío, la cámara estará estática.")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Configuración de Velocidad")]
    public float customDuration = 1.0f;
    public bool smoothEaseInOut = true;

    [Header("Ajustes especiales")]
    [Tooltip("Actívalo SOLO en tu Main Game Camera para evitar tirones o movimientos bruscos al activarse")]
    public bool isMainCamera = false;

    private float duration = 1f;
    private float elapsedTime = 0f;
    private bool isMoving = false;
    private Camera camComponent;

    void Awake()
    {
        camComponent = GetComponent<Camera>();
    }

    public void PlayMovement(float overrideDuration)
    {
        duration = (customDuration > 0) ? customDuration : overrideDuration;
        elapsedTime = 0f;

        // Si tiene puntos asignados, se mueve. Si no, se queda quieta en su sitio.
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    void Update()
    {
        if (!isMoving) return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / duration);

        if (smoothEaseInOut)
        {
            t = t * t * (3f - 2f * t);
        }

        if (startPoint != null && endPoint != null)
        {
            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
            transform.rotation = Quaternion.Slerp(startPoint.rotation, endPoint.rotation, t);
        }

        if (elapsedTime >= duration)
        {
            isMoving = false;
        }
    }

    void OnDrawGizmos()
    {
        if (startPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(startPoint.position, Vector3.one * 0.5f);
        }

        if (endPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(endPoint.position, Vector3.one * 0.5f);
        }

        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }
    }
}