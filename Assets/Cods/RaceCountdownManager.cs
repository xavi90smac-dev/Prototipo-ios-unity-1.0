using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceCountdownManager : MonoBehaviour
{
    [Header("Cámaras Cinemáticas de Conteo")]
    public Camera cameraThree;
    public Camera cameraTwo;
    public Camera mainGameCamera;

    [Header("Textos Personalizados del Conteo")]
    public string textForThree = "3";
    public string textForTwo = "2";
    public string textForOne = "1";
    public string textForGo = "¡YA!";

    [Header("Interfaz de Usuario (UI)")]
    public TMP_Text countdownText;

    [Header("Control de Vehículos")]
    public MonoBehaviour playerInputScript; 
    public List<CarReplay> rivalReplays;

    void Start()
    {
        StartCoroutine(RunCountdownSequence());
    }

    IEnumerator RunCountdownSequence()
    {
        // 1. Congelar juego al inicio
        SetGamePlayable(false);

        // --- FASE 3 ---
        float duration3 = GetCameraDuration(cameraThree, 1.0f);
        ActivateTargetCamera(cameraThree, duration3);
        if (countdownText) countdownText.text = textForThree;
        yield return new WaitForSeconds(duration3);

        // --- FASE 2 ---
        float duration2 = GetCameraDuration(cameraTwo, 1.0f);
        ActivateTargetCamera(cameraTwo, duration2);
        if (countdownText) countdownText.text = textForTwo;
        yield return new WaitForSeconds(duration2);

        // --- FASE 1 (Usa la cámara principal o una fija si prefieres) ---
        float duration1 = GetCameraDuration(mainGameCamera, 1.0f);
        ActivateTargetCamera(mainGameCamera, duration1);
        if (countdownText) countdownText.text = textForOne;
        yield return new WaitForSeconds(duration1);

        // --- ¡YA! ---
        if (countdownText) countdownText.text = textForGo;
        
        // 2. Arrancar carrera
        SetGamePlayable(true);

        yield return new WaitForSeconds(0.7f);

        if (countdownText) countdownText.gameObject.SetActive(false);
    }

    float GetCameraDuration(Camera cam, float defaultTime)
    {
        if (cam != null)
        {
            CinematicCameraMove mover = cam.GetComponent<CinematicCameraMove>();
            if (mover != null)
            {
                // Retorna la duración personalizada que pusiste en el Inspector de esa cámara
                return mover.customDuration;
            }
        }
        return defaultTime;
    }

    void ActivateTargetCamera(Camera activeCam, float duration)
    {
        if (cameraThree) cameraThree.gameObject.SetActive(false);
        if (cameraTwo) cameraTwo.gameObject.SetActive(false);
        if (mainGameCamera) mainGameCamera.gameObject.SetActive(false);

        if (activeCam)
        {
            activeCam.gameObject.SetActive(true);
            
            CinematicCameraMove mover = activeCam.GetComponent<CinematicCameraMove>();
            if (mover != null)
            {
                mover.PlayMovement(duration);
            }
        }
    }

    void SetGamePlayable(bool playable)
    {
        if (playerInputScript != null)
        {
            playerInputScript.enabled = playable;
        }

        if (playable)
        {
            foreach (var rival in rivalReplays)
            {
                if (rival != null)
                {
                    rival.BeginRace();
                }
            }
        }
    }
}