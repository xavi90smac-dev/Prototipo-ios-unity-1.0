using UnityEngine;

public class WheelSpin : MonoBehaviour
{
    public Transform[] wheels; // Arrastra aquí los objetos hijos de las llantas
    public float wheelRadius = 0.35f; // Ajusta según el tamaño de tu rueda
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Calcula la distancia que se movió el carro en este frame
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        if (distanceMoved > 0f)
        {
            // Calcula los grados de giro basados en el perímetro de la rueda
            float rotationAngle = (distanceMoved / (2f * Mathf.PI * wheelRadius)) * 360f;

            // Gira cada llanta sobre su propio eje
            foreach (Transform wheel in wheels)
            {
                if (wheel != null)
                {
                    wheel.Rotate(Vector3.right, rotationAngle, Space.Self);
                }
            }
        }
    }
}