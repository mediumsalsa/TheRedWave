using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEffects : MonoBehaviour
{
    public Camera mainCamera;
    private Vector3 originalPosition;

    [Header("Default Settings")]
    [SerializeField] private float defaultShakeDuration = 0.2f;
    [SerializeField] private float defaultShakeIntensity = 0.2f;
    [SerializeField] private float defaultFreezeDuration = 0.1f;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            originalPosition = mainCamera.transform.position;
        }
        else
        {
            Debug.LogError("Main camera not found in the scene!");
        }
    }

    // Public method to apply screen shake
    public void ScreenShake(float duration = -1, float intensity = -1)
    {
        if (mainCamera != null)
        {
            float shakeDuration = duration > 0 ? duration : defaultShakeDuration;
            float shakeIntensity = intensity > 0 ? intensity : defaultShakeIntensity;

            //Debug.Log($"ScreenShake triggered with duration: {shakeDuration}, intensity: {shakeIntensity}");
            StartCoroutine(Shake(shakeDuration, shakeIntensity));
        }
        else
        {
            Debug.LogError("No Camera assigned for screen shake!");
        }
    }

    private IEnumerator Shake(float duration, float intensity)
    {
        originalPosition = mainCamera.transform.position; // Dynamically set the original position
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                0
            );

            mainCamera.transform.position = originalPosition + randomOffset;
            elapsed += Time.unscaledDeltaTime; // Use unscaledDeltaTime for freeze compatibility
            yield return null;
        }

        mainCamera.transform.position = originalPosition; // Reset to original position
    }

    // Public method to apply freeze frame
    public void FreezeFrame(float duration = -1)
    {
        float freezeDuration = duration > 0 ? duration : defaultFreezeDuration;
        StartCoroutine(Freeze(freezeDuration));
    }

    private IEnumerator Freeze(float duration)
    {
        if (Time.timeScale != 0)
        {
            //Debug.Log($"FreezeFrame triggered for duration: {duration}");
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0f; // Pause the game
            yield return new WaitForSecondsRealtime(duration); // Use unscaled time
            Time.timeScale = originalTimeScale; // Restore time scale
        }
    }
}
