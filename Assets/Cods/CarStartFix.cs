using UnityEngine;

public class CarStartFix : MonoBehaviour
{
    // El script de movimiento/entrada que se desactivará al inicio
    public MonoBehaviour movementInputScript;

    void Awake()
    {
        if (movementInputScript == null)
        {
            movementInputScript = GetComponent("StandardInput") as MonoBehaviour;
        }

        // Desactivamos ÚNICAMENTE el movimiento al iniciar la escena.
        // La suspensión y los Wheel Colliders se quedan activos desde el fotograma 1.
        if (movementInputScript != null)
        {
            movementInputScript.enabled = false;
        }
    }

    // Esta función pública se encargará de liberar el carro
    public void ReleaseCar()
    {
        if (movementInputScript != null)
        {
            movementInputScript.enabled = true;
        }
    }
}