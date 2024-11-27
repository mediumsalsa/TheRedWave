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

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;
    private bool isKnockedBack;

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
            StartCoroutine(Shake(shakeDuration, shakeIntensity));
        }
        else
        {
            Debug.LogError("No Camera assigned for screen shake!");
        }
    }

    private IEnumerator Shake(float duration, float intensity)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                0
            );

            mainCamera.transform.position = originalPosition + randomOffset;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPosition;
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
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = originalTimeScale;
        }
    }

    // Public method to apply hit effects
    public void TriggerHitEffects(GameObject target, Vector3 enemyPosition, bool isKnocked_Back)
    {
        StartCoroutine(HitEffectsRoutine(target, enemyPosition, isKnockedBack));
    }

    private IEnumerator HitEffectsRoutine(GameObject target, Vector3 enemyPosition, bool isKnocked_Back)
    {
        // Flash effect
        StartCoroutine(HitFlash(target));

        // Knockback
        Vector2 knockbackDir = (target.transform.position - enemyPosition).normalized;
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            StartCoroutine(ApplyKnockback(rb, knockbackDir, isKnocked_Back));
        }

        // Freeze and shake
        FreezeFrame();
        ScreenShake();
        yield return null;
    }

    // Hit flash visual effect
    private IEnumerator HitFlash(GameObject target)
    {
        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            material.SetFloat("_FlashAmount", 1);
            yield return new WaitForSeconds(0.05f);
            material.SetFloat("_FlashAmount", 0);
        }
    }

    // Apply knockback force
    private IEnumerator ApplyKnockback(Rigidbody2D rb, Vector2 direction, bool is_KnockedBack)
    {
    if (rb == null)
    {
        Debug.LogWarning("Rigidbody2D is null. Knockback cancelled.");
        yield break;
    }
    isKnockedBack = true;
    is_KnockedBack = true;
    float timer = 0f;

    while (timer < knockbackDuration)
    {
        if (rb == null) // Check if Rigidbody2D is destroyed during the loop
        {
            Debug.LogWarning("Rigidbody2D was destroyed during knockback.");
            yield break;
        }

        rb.velocity = direction * knockbackForce;
        timer += Time.fixedDeltaTime;
        yield return new WaitForFixedUpdate();
    }

    if (rb != null) // Safeguard against null before resetting velocity
    {
        rb.velocity = Vector2.zero;
    }
    isKnockedBack = false;
    is_KnockedBack = false;
    }
}
