using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RivalEngineSound : MonoBehaviour
{
    [Header("Configuración de Distancia")]
    [Tooltip("Distancia a partir de la cual el carro comienza a escucharse.")]
    public float minDistance = 2f;
    [Tooltip("Distancia máxima donde el sonido deja de escucharse por completo.")]
    public float maxDistance = 25f;

    [Header("Ajustes de Sonido")]
    [Range(0f, 1f)]
    [Tooltip("Volumen general del motor para este rival.")]
    public float masterVolume = 0.7f;

    [Tooltip("Velocidad máxima estimada del rival para escalar el tono del motor.")]
    public float maxSpeedForPitch = 30f;

    [Header("Tono (Pitch)")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.8f;

    private AudioSource audioSource;
    private Rigidbody rb;
    private Vector3 lastPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;

        // Configuraciones clave para que el AudioSource sea espacial y realista
        audioSource.spatialBlend = 1.0f; // Sonido 3D total en el espacio
        audioSource.rolloffMode = AudioRolloffMode.Custom; // Curva de distancia personalizada
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.loop = true;

        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        if (audioSource == null) return;

        // Actualizamos las distancias en tiempo real por si las modificas en el Inspector
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;

        // Calculamos la velocidad real del carro cuadro por cuadro (independiente de si es cinemático)
        float currentSpeed = 0f;
        if (rb != null)
        {
            currentSpeed = rb.linearVelocity.magnitude;
        }
        else
        {
            currentSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
            lastPosition = transform.position;
        }

        // Calculamos el tono del motor de forma progresiva y realista
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeedForPitch);
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedFactor);
        
        // Suavizamos el cambio de tono para evitar saltos bruscos de audio
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, Time.deltaTime * 5f);

        // Ajustamos el volumen general aplicando el control maestro del Inspector
        // El propio AudioSource de Unity se encarga de atenuar según la distancia (min/maxDistance)
        float distanceToPlayer = Camera.main != null ? Vector3.Distance(transform.position, Camera.main.transform.position) : 0f;
        
        if (distanceToPlayer > maxDistance)
        {
            audioSource.volume = 0f;
        }
        else
        {
            // Atenuación cúbica suave para que se escuche solo cuando esté muy cerca
            float distanceFactor = 1f - Mathf.Clamp01(distanceToPlayer / maxDistance);
            audioSource.volume = masterVolume * (distanceFactor * distanceFactor);
        }
    }
}