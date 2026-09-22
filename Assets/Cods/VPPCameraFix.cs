using UnityEngine;

public class VPPCameraFix : MonoBehaviour
{
    [Header("Referencia al Carro del Jugador")]
    [Tooltip("Arrastra aquí el Transform de tu carro principal (ej. AUD1)")]
    public Transform targetCar;

    [Header("Ajustes de Distancia (Igual que tu VPP Smooth Follow)")]
    public float distance = 9.1f;
    public float height = 4.0f;

    // Se ejecuta automáticamente cada vez que la Main Camera se enciende
    void OnEnable()
    {
        SnapCameraToTarget();
    }

    public void SnapCameraToTarget()
    {
        if (targetCar != null)
        {
            // Calcula de inmediato la posición exacta detrás del carro y coloca la cámara ahí sin interpolar
            Vector3 idealPosition = targetCar.position - (targetCar.forward * distance) + (Vector3.up * height);
            transform.position = idealPosition;
            transform.LookAt(targetCar.position + Vector3.up * 1.5f);
        }
    }
}