using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CarRecorder : MonoBehaviour
{
    [Header("Teclas de Grabación para cada Rival")]
    [Tooltip("Tecla para iniciar la grabación del Rival 1")]
    public KeyCode recordKeyRival1 = KeyCode.Z;

    [Tooltip("Tecla para iniciar la grabación del Rival 2")]
    public KeyCode recordKeyRival2 = KeyCode.V;

    [Tooltip("Tecla para iniciar la grabación del Rival 3 (Futuro)")]
    public KeyCode recordKeyRival3 = KeyCode.B;

    [Tooltip("Tecla para iniciar la grabación del Rival 4 (Futuro)")]
    public KeyCode recordKeyRival4 = KeyCode.N;

    [Header("Tecla Global")]
    [Tooltip("Tecla para detener y guardar la grabación activa")]
    public KeyCode stopKey = KeyCode.X;

    private List<FrameData> recordedFrames = new List<FrameData>();
    private bool isRecording = false;
    private float recordingTimer = 0f;
    private string targetFileName = "";

    void Update()
    {
        // Detectar comandos según las teclas asignadas en el Inspector
        if (Input.GetKeyDown(recordKeyRival1))
        {
            targetFileName = "rival_recording_1.json";
            StartRecording();
        }
        else if (Input.GetKeyDown(recordKeyRival2))
        {
            targetFileName = "rival_recording_2.json";
            StartRecording();
        }
        else if (Input.GetKeyDown(recordKeyRival3))
        {
            targetFileName = "rival_recording_3.json";
            StartRecording();
        }
        else if (Input.GetKeyDown(recordKeyRival4))
        {
            targetFileName = "rival_recording_4.json";
            StartRecording();
        }

        // Detener y guardar
        if (Input.GetKeyDown(stopKey) && isRecording)
        {
            StopAndSaveRecording();
        }
    }

    void FixedUpdate()
    {
        if (isRecording)
        {
            recordingTimer += Time.fixedDeltaTime;
            recordedFrames.Add(new FrameData {
                time = recordingTimer,
                position = transform.position,
                rotation = transform.rotation
            });
        }
    }

    void StartRecording()
    {
        recordedFrames.Clear();
        recordingTimer = 0f;
        isRecording = true;
        Debug.Log("🔴 GRABANDO VUELTA para: " + targetFileName + " ... ¡Conduce hacia la meta!");
    }

    void StopAndSaveRecording()
    {
        isRecording = false;

        RecordingData data = new RecordingData();
        data.frames = recordedFrames;

        string json = JsonUtility.ToJson(data);
        string path = Application.persistentDataPath + "/" + targetFileName;
        File.WriteAllText(path, json);

        Debug.Log("💾 ¡Grabación guardada con éxito en: " + path);
    }
}