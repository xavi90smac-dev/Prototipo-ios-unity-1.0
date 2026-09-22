using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FrameData
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class RecordingData
{
    public List<FrameData> frames = new List<FrameData>();
}