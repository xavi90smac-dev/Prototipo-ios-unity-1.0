using UnityEngine;
using System.Collections.Generic;

public class TyreExplosion : MonoBehaviour
{
    public float explosionForce = 500f; // Fuerza hacia afuera
    public float upwardModifier = 3f;     // Fuerza extra hacia arriba
    public float explosionRadius = 2f;    // Rango de explosión
    
    private List<Rigidbody> tyreRigidbodies = new List<Rigidbody>();
    private bool hasExploded = false;

    void Start()
    {
        // Encuentra todos los rigidbodies de las llantas hijas al iniciar
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                tyreRigidbodies.Add(rb);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Si choca el carro y no ha explotado ya
        if (!hasExploded && collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        // Desactiva la colisión entre las llantas hijas para que no se estorben al explotar
        foreach (Rigidbody rb in tyreRigidbodies)
        {
            rb.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // O crea una capa específica para ignorar colisiones entre llantas
        }

        // Aplica una fuerza explosiva física a cada llanta individual
        Vector3 explosionCenter = transform.position;

        foreach (Rigidbody rb in tyreRigidbodies)
        {
            // Asegúrate de que las llantas tengan gravedad y no sean cinemáticas
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddExplosionForce(explosionForce, explosionCenter, explosionRadius, upwardModifier, ForceMode.Impulse);
        }
        
        // Opcional: Destruye el objeto padre después de un tiempo para limpiar la escena
        Destroy(gameObject, 5f); 
    }
}