using UnityEngine;

public class MobileTouchController : MonoBehaviour
{
    private Component standardInput;
    
    private float throttleInput = 0f;
    private float steerInput = 0f;
    private float brakeInput = 0f;
    private float handbrakeInput = 0f;

    void Start()
    {
        // Buscamos VPStandardInput que es el nombre exacto de Vehicle Physics Pro
        standardInput = GetComponent("VPStandardInput") ?? GetComponent("StandardInput");
        
        if (standardInput == null)
        {
            Debug.LogError("¡No se encontró el componente VPStandardInput en el carro AUD1!");
        }
    }

    void Update()
    {
        if (standardInput != null)
        {
            System.Type type = standardInput.GetType();
            type.GetField("externalThrottle")?.SetValue(standardInput, throttleInput);
            type.GetField("externalSteer")?.SetValue(standardInput, steerInput);
            type.GetField("externalBrake")?.SetValue(standardInput, brakeInput);
            type.GetField("externalHandbrake")?.SetValue(standardInput, handbrakeInput);
        }
    }

    // --- DIRECCIÓN ---
    public void PressLeft() { steerInput = -1f; }
    public void PressRight() { steerInput = 1f; }
    public void ReleaseSteer() { steerInput = 0f; }

    // --- ACELERADOR ---
    public void PressThrottle() { throttleInput = 1f; }
    public void ReleaseThrottle() { throttleInput = 0f; }

    // --- FRENO / REVERSA ---
    public void PressBrake() { throttleInput = -1f; brakeInput = 1f; }
    public void ReleaseBrake() { throttleInput = 0f; brakeInput = 0f; }

    // --- FRENO DE MANO ---
    public void PressHandbrake() { handbrakeInput = 1f; }
    public void ReleaseHandbrake() { handbrakeInput = 0f; }

    // --- CAMBIO DE CÁMARA UNIVERSAL ---
    public void ChangeCamera()
    {
        bool cameraChanged = false;

        // Buscamos cualquier componente que controle la cámara en el vehículo
        foreach (Component comp in GetComponentsInChildren<Component>(true))
        {
            if (comp != null)
            {
                string compName = comp.GetType().Name;
                
                // Verificamos si es el controlador de VPP o un script de cámara genérico
                if (compName.Contains("Camera") || compName.Contains("VPCamera"))
                {
                    System.Type type = comp.GetType();
                    
                    // Buscamos métodos comunes para cambiar de cámara
                    System.Reflection.MethodInfo method = type.GetMethod("ChangeCamera") ?? 
                                                          type.GetMethod("CycleCamera") ?? 
                                                          type.GetMethod("NextCamera");
                    if (method != null)
                    {
                        method.Invoke(comp, null);
                        cameraChanged = true;
                        break;
                    }

                    // O buscamos campos de índice para incrementarlos
                    System.Reflection.FieldInfo field = type.GetField("customCameraIndex") ?? 
                                                        type.GetField("m_cameraIndex") ?? 
                                                        type.GetField("cameraIndex");
                    if (field != null)
                    {
                        int currentIndex = (int)field.GetValue(comp);
                        field.SetValue(comp, currentIndex + 1);
                        cameraChanged = true;
                        break;
                    }
                }
            }
        }

        if (cameraChanged)
        {
            Debug.Log("¡Cámara cambiada exitosamente!");
        }
        else
        {
            Debug.LogWarning("No se encontró ningún método de cambio de cámara compatible automáticamente.");
        }
    }
}