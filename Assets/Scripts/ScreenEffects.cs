using Pathfinding;
using System.Collections;
using UnityEngine;

public class ScreenEffects : MonoBehaviour
{
    private Vector3 originalPosition;

    [Header("Default Settings")]
    [SerializeField] private float defaultShakeDuration = 0.2f;
    [SerializeField] private float defaultShakeIntensity = 0.2f;
    [SerializeField] private float defaultFreezeDuration = 0.1f;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;

    public IEnumerator ApplyKnockback(Rigidbody2D rb, Vector2 direction, Entity entity)
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is null. Knockback cancelled.");
            yield break;
        }

        direction = direction.normalized;
        AILerp aiLerp = entity.GetComponent<AILerp>();

        if (aiLerp != null) aiLerp.enabled = false;

        entity.isKnockedBack = true;

        // Apply knockback using AddForce (Impulse)
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.AddForce(direction * entity.knockbackForce, ForceMode2D.Impulse);
            Debug.Log($"Applied knockback force: {direction * entity.knockbackForce}");
        }

        yield return new WaitForSeconds(entity.knockbackDuration);

        // Stop movement after knockback
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.velocity = Vector2.zero;
            Debug.Log("Knockback ended. Velocity reset.");
        }
        if (aiLerp != null) aiLerp.enabled = true;

        entity.isKnockedBack = false;
    }

    public IEnumerator ApplyIframes(Entity targetEntity, float iframeDuration, float FlashDuration)
    {
        if (targetEntity == null) yield break;

        targetEntity.areIFrames = true;

        SpriteRenderer renderer = targetEntity.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Material material = renderer.material;

            if (material.HasProperty("_FlashAmount"))
            {
                material.SetFloat("_FlashAmount", 1);
                yield return new WaitForSeconds(FlashDuration);
                material.SetFloat("_FlashAmount", 0);
            }
        }
        targetEntity.areIFrames = false;
    }

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
}
