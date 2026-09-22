using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TyrePhysics : MonoBehaviour
{
    public float hitMultiplier = 1.5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 25f; 
        rb.linearDamping = 0.5f; 
        rb.angularDamping = 0.5f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.relativeVelocity.magnitude > 3f)
        {
            Vector3 impactDirection = collision.relativeVelocity;
            rb.AddForce(impactDirection * hitMultiplier, ForceMode.Impulse);
        }
    }
}