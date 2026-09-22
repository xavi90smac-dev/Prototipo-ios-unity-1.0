using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CarReplay : MonoBehaviour
{
    [Header("Archivo de Grabación")]
    public string recordingFileName = "rival_recording_1.json"; // Cambia según el rival (rival_recording_2.json, etc.)

    private List<FrameData> playbackFrames = new List<FrameData>();
    private bool isPlaying = false;
    private float playbackTimer = 0f;
    private int currentIndex = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        LoadRecording();

        if (rb != null)
        {
            rb.isKinematic = true; // Se mantiene estático hasta que comience la carrera
        }
    }

    // Método público para iniciar la reproducción al terminar la cuenta
    public void BeginRace()
    {
        StartPlayback();
    }

    void LoadRecording()
    {
        string path = Application.persistentDataPath + "/" + recordingFileName;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            RecordingData data = JsonUtility.FromJson<RecordingData>(json);
            playbackFrames = data.frames;
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró el archivo: " + recordingFileName);
        }
    }

    public void StartPlayback()
    {
        if (playbackFrames.Count > 0)
        {
            playbackTimer = 0f;
            currentIndex = 0;
            isPlaying = true;
        }
    }

    void FixedUpdate()
    {
        if (!isPlaying || playbackFrames.Count == 0 || rb == null) return;

        playbackTimer += Time.fixedDeltaTime;

        while (currentIndex < playbackFrames.Count && playbackFrames[currentIndex].time <= playbackTimer)
        {
            rb.MovePosition(playbackFrames[currentIndex].position);
            rb.MoveRotation(playbackFrames[currentIndex].rotation);
            currentIndex++;
        }

        if (currentIndex >= playbackFrames.Count)
        {
            isPlaying = false;
        }
    }
}