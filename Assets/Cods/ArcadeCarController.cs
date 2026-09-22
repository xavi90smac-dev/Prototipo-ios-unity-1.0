using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("=== VELOCIDAD Y MOTOR ===")]
    public float maxSpeed = 22f;
    public float acceleration = 16f;
    public float deceleration = 6f;
    public float turnSpeed = 80f;

    [Header("=== DERRAPE Y DOBLE TOQUE (TIGHT DRIFT) ===")]
    public KeyCode driftKey = KeyCode.Space;
    public float normalGrip = 16f;
    public float driftGrip = 4.2f;
    public float tightDriftGrip = 1.5f;
    public float tightDriftTurnMultiplier = 2.0f; // Multiplicador de giro fuerte en el segundo toque

    [Range(0.5f, 3.0f)]
    public float driftSlideControl = 1.2f;

    [Header("=== REFERENCIAS DE RUEDAS ===")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [Header("=== CORRECCIÓN Y EJES DE GIRO ===")]
    public SteeringAxis steeringAxis = SteeringAxis.Y;
    public bool invertLeftWheel = false;

    [Header("=== ROTACIÓN DE AVANCE (RUEDAS) ===")]
    public float wheelRadius = 0.35f;
    public bool invertWheelRoll = false;

    [Header("=== SISTEMA DE AUDIO MULTICAPA ===")]
    public AudioSource idleAudioSource;
    public AudioSource highSpeedLoopSource;
    public AudioSource turboAudioSource;
    public AudioSource skidAudioSource;

    [Header("=== ARCHIVOS DE AUDIO (CLIPS) ===")]
    public AudioClip idleClip;
    public AudioClip highSpeedLoopClip;
    public AudioClip turboClip;

    [Header("=== CONFIGURACIÓN DE AUDIO ===")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.4f;
    [Range(0f, 1f)] public float maxSkidVolume = 0.5f;

    public enum SteeringAxis { X, Y, Z }

    private Rigidbody rb;
    private float currentSpeed = 0f;
    [HideInInspector] public float currentSteer = 0f;
    private float wheelRotationAngle = 0f;
    private bool isDrifting = false;
    private bool isTightDrifting = false;
    private float mobileGasInput = 0f;
    [HideInInspector] public float driftIntensity = 0f;
    private bool wasAccelerating = false;
    private float simulatedRpm = 0f;

    private Quaternion leftWheelInitialRot;
    private Quaternion rightWheelInitialRot;
    private Quaternion rearLeftInitialRot;
    private Quaternion rearRightInitialRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.centerOfMass = new Vector3(0, -0.6f, 0.0f);
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.angularDamping = 4.0f;
        }

        // --- SOLUCIÓN PARA VEGETACIÓN Y OBSTÁCULOS: Material físico sin fricción lateral ni rebote ---
        Collider carCollider = GetComponent<Collider>();
        if (carCollider != null)
        {
            PhysicsMaterial frictionlessMat = new PhysicsMaterial("CarAntiSnagMaterial");
            frictionlessMat.staticFriction = 0f;
            frictionlessMat.dynamicFriction = 0f;
            frictionlessMat.bounciness = 0f;
            frictionlessMat.frictionCombine = PhysicsMaterialCombine.Minimum;
            frictionlessMat.bounceCombine = PhysicsMaterialCombine.Minimum;
            carCollider.sharedMaterial = frictionlessMat;
        }

        if (frontLeftWheel != null) leftWheelInitialRot = frontLeftWheel.localRotation;
        if (frontRightWheel != null) rightWheelInitialRot = frontRightWheel.localRotation;
        if (rearLeftWheel != null) rearLeftInitialRot = rearLeftWheel.localRotation;
        if (rearRightWheel != null) rearRightInitialRot = rearRightWheel.localRotation;

        SetupAudioSources();
    }

    void SetupAudioSources()
    {
        if (idleAudioSource != null && idleClip != null)
        {
            idleAudioSource.clip = idleClip;
            idleAudioSource.loop = true;
            idleAudioSource.volume = 1f;
            idleAudioSource.Play();
        }

        if (highSpeedLoopSource != null && highSpeedLoopClip != null)
        {
            highSpeedLoopSource.clip = highSpeedLoopClip;
            highSpeedLoopSource.loop = true;
            highSpeedLoopSource.volume = 0f;
            highSpeedLoopSource.Play();
        }

        if (skidAudioSource != null)
        {
            skidAudioSource.loop = true;
            skidAudioSource.volume = 0f;
            if (!skidAudioSource.isPlaying) skidAudioSource.Play();
        }
    }

    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float moveInput = (Mathf.Abs(mobileGasInput) > 0.01f) ? mobileGasInput : verticalInput;
        
        float steerInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(driftKey))
        {
            if (!isDrifting)
            {
                TriggerHandbrake();
            }
            else if (isDrifting && !isTightDrifting)
            {
                isTightDrifting = true; // Segundo toque para cerrar el ángulo de golpe
            }
        }

        if (isDrifting)
        {
            driftIntensity = Mathf.MoveTowards(driftIntensity, isTightDrifting ? 1.5f : 1f, 5f * Time.deltaTime);
            
            // El drift termina inmediatamente si sueltas el espacio, si bajas mucho la velocidad, o si enderezas el volante
            if (Input.GetKeyUp(driftKey) || Mathf.Abs(currentSpeed) < 3f || (Mathf.Abs(steerInput) < 0.1f && Mathf.Abs(rb.linearVelocity.x) < 0.8f))
            {
                EndDrift();
            }
        }
        else
        {
            driftIntensity = Mathf.MoveTowards(driftIntensity, 0f, 3f * Time.deltaTime);
            isTightDrifting = false;

            if (Mathf.Abs(steerInput) > 0.85f && Mathf.Abs(currentSpeed) > 14f && Input.GetKey(driftKey))
            {
                TriggerHandbrake();
            }
        }

        // Suavizado de velocidad base controlado por input
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            float currentAccel = isDrifting ? (acceleration * 0.9f) : acceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, moveInput * maxSpeed, currentAccel * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }

        currentSteer = Mathf.Lerp(currentSteer, steerInput, 10f * Time.deltaTime);

        float rollMultiplier = invertWheelRoll ? -1f : 1f;
        wheelRotationAngle += (currentSpeed / wheelRadius) * Mathf.Rad2Deg * Time.deltaTime * rollMultiplier;

        AnimateWheels();
        UpdateCustomEngineAudio(moveInput);
        UpdateRealisticSkidAudio();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        float verticalInput = Input.GetAxis("Vertical");
        float moveInput = (Mathf.Abs(mobileGasInput) > 0.01f) ? mobileGasInput : verticalInput;

        // Sincronización exacta en FixedUpdate
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            float currentAccel = isDrifting ? (acceleration * 0.9f) : acceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, moveInput * maxSpeed, currentAccel * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        if (Mathf.Abs(currentSpeed) > 0.2f)
        {
            float turnDirection = Mathf.Sign(currentSpeed);
            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / 5f);
            
            float activeTurnSpeed = turnSpeed;
            if (isDrifting)
            {
                activeTurnSpeed *= 1.2f * driftSlideControl;
            }
            if (isTightDrifting)
            {
                activeTurnSpeed *= tightDriftTurnMultiplier;
            }

            float rotationAmount = currentSteer * activeTurnSpeed * speedFactor * turnDirection * Time.fixedDeltaTime;
            Quaternion turnOffset = Quaternion.Euler(0f, rotationAmount, 0f);
            rb.MoveRotation(rb.rotation * turnOffset);
        }

        Vector3 velocity = rb.linearVelocity;
        Vector3 forwardDir = transform.forward;
        Vector3 rightDir = transform.right;

        float forwardSpeed = Vector3.Dot(velocity, forwardDir);
        float lateralSpeed = Vector3.Dot(velocity, rightDir);

        float targetForward = Mathf.MoveTowards(forwardSpeed, currentSpeed, (isDrifting ? acceleration * 0.9f : acceleration) * Time.fixedDeltaTime);
        
        float currentGrip = normalGrip;
        if (isDrifting) currentGrip = driftGrip;
        if (isTightDrifting) currentGrip = tightDriftGrip;

        float dampedLateralSpeed = Mathf.Lerp(lateralSpeed, 0f, currentGrip * Time.fixedDeltaTime);

        Vector3 targetVelocity = (forwardDir * targetForward) + (rightDir * dampedLateralSpeed);
        targetVelocity.y = velocity.y;

        float velocityLerpRate = isDrifting ? 20f : 16f;
        rb.linearVelocity = Vector3.Lerp(velocity, targetVelocity, velocityLerpRate * Time.fixedDeltaTime);
    }

    void UpdateCustomEngineAudio(float moveInput)
    {
        float speedPercentage = Mathf.Abs(currentSpeed) / maxSpeed;
        float inputIntensity = Mathf.Abs(moveInput);

        float targetRpmFactor = speedPercentage;
        if (inputIntensity > 0.05f)
        {
            targetRpmFactor = Mathf.Max(speedPercentage, 0.5f);
        }

        if (isDrifting)
        {
            targetRpmFactor = 0.5f;
        }
        else if (inputIntensity < 0.05f)
        {
            targetRpmFactor = Mathf.Min(targetRpmFactor, speedPercentage * 0.4f);
        }

        simulatedRpm = Mathf.MoveTowards(simulatedRpm, targetRpmFactor, 4f * Time.deltaTime);

        // 1. IDLE
        if (idleAudioSource != null)
        {
            float targetIdleVol = (simulatedRpm < 0.15f && inputIntensity < 0.15f) ? 1f : 0f;
            idleAudioSource.volume = Mathf.MoveTowards(idleAudioSource.volume, targetIdleVol, 5f * Time.deltaTime);
        }

        // 2. LOOP PRINCIPAL
        if (highSpeedLoopSource != null)
        {
            float targetLoopVol = (inputIntensity > 0.05f || simulatedRpm > 0.05f) ? 0.6f : 0f;
            if (isDrifting) targetLoopVol = 0.55f;

            highSpeedLoopSource.volume = Mathf.MoveTowards(highSpeedLoopSource.volume, targetLoopVol, 5f * Time.deltaTime);
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, simulatedRpm);
            highSpeedLoopSource.pitch = Mathf.MoveTowards(highSpeedLoopSource.pitch, targetPitch, 6f * Time.deltaTime);
        }

        // 3. TURBO UNIVERSAL
        bool isCurrentlyAccelerating = inputIntensity > 0.2f;

        if (wasAccelerating && inputIntensity < 0.05f)
        {
            if (turboClip != null && turboAudioSource != null)
            {
                turboAudioSource.PlayOneShot(turboClip, 1.0f);
            }
        }
        wasAccelerating = isCurrentlyAccelerating;
    }

    void UpdateRealisticSkidAudio()
    {
        if (skidAudioSource == null) return;

        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float lateralSlip = Mathf.Abs(localVelocity.x);

        bool isSliding = isDrifting || (lateralSlip > 2.5f && Mathf.Abs(currentSpeed) > 5f);

        float targetSkidVolume = 0f;
        if (isSliding)
        {
            targetSkidVolume = Mathf.Clamp01(lateralSlip / 8f) * maxSkidVolume;
            if (isDrifting) targetSkidVolume = Mathf.Max(targetSkidVolume, maxSkidVolume * 0.8f);
        }

        skidAudioSource.volume = Mathf.MoveTowards(skidAudioSource.volume, targetSkidVolume, 10f * Time.deltaTime);

        if (skidAudioSource.isPlaying)
        {
            skidAudioSource.pitch = Mathf.Lerp(0.8f, 1.3f, lateralSlip / 10f);
        }
    }

    public void TriggerHandbrake()
    {
        if (Mathf.Abs(currentSpeed) > 3f)
        {
            isDrifting = true;
            isTightDrifting = false;
            rb.AddForce(transform.right * 2.5f * Mathf.Sign(currentSteer != 0 ? currentSteer : 1f), ForceMode.VelocityChange);
        }
    }

    void EndDrift()
    {
        isDrifting = false;
        isTightDrifting = false;
        if (rb != null)
        {
            currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        }
    }

    public void SetMobileGas(float gasAmount)
    {
        mobileGasInput = gasAmount;
    }

    void AnimateWheels()
    {
        float targetSteerAngle = currentSteer * 32f;
        Quaternion rollRot = Quaternion.Euler(wheelRotationAngle, 0f, 0f);

        if (frontRightWheel != null)
        {
            Quaternion steerRot = GetSteerRotation(targetSteerAngle);
            frontRightWheel.localRotation = rightWheelInitialRot * steerRot * rollRot;
        }

        if (frontLeftWheel != null) {
            float leftMultiplier = invertLeftWheel ? -1f : 1f;
            Quaternion steerRot = GetSteerRotation(targetSteerAngle * leftMultiplier);
            frontLeftWheel.localRotation = leftWheelInitialRot * steerRot * rollRot;
        }

        if (rearRightWheel != null) rearRightWheel.localRotation = rearLeftInitialRot * rollRot;
        if (rearLeftWheel != null) rearLeftInitialRot = rearLeftInitialRot * rollRot;
    }

    Quaternion GetSteerRotation(float angle)
    {
        switch (steeringAxis)
        {
            case SteeringAxis.X: return Quaternion.Euler(angle, 0f, 0f);
            case SteeringAxis.Y: return Quaternion.Euler(0f, angle, 0f);
            case SteeringAxis.Z: return Quaternion.Euler(0f, 0f, angle);
            default: return Quaternion.Euler(0f, angle, 0f);
        }
    }
}