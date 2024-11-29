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

        // Normalize direction and apply knockback force
        direction = direction.normalized;

        // Disable AI movement for enemies
        AILerp aiLerp = entity.GetComponent<AILerp>();
        if (aiLerp != null) aiLerp.enabled = false;

        // Disable player/enemy-specific movement logic
        entity.isKnockedBack = true;

        // Apply knockback using AddForce (Impulse)
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.AddForce(direction * entity.knockbackForce, ForceMode2D.Impulse);
            Debug.Log($"Applied knockback force: {direction * entity.knockbackForce}");
        }

        // Wait for knockback duration
        yield return new WaitForSeconds(entity.knockbackDuration);

        // Stop movement after knockback
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.velocity = Vector2.zero;
            Debug.Log("Knockback ended. Velocity reset.");
        }

        // Re-enable AI movement for enemies
        if (aiLerp != null) aiLerp.enabled = true;

        // Re-enable player/enemy-specific movement
        entity.isKnockedBack = false;
    }

    public IEnumerator ApplyIframes(Entity targetEntity, float iframeDuration, Color flashColor)
    {
        if (targetEntity == null) yield break;

        SpriteRenderer renderer = targetEntity.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.color;

            for (float elapsed = 0; elapsed < iframeDuration; elapsed += 0.1f)
            {
                renderer.color = flashColor;
                yield return new WaitForSeconds(0.05f);
                renderer.color = originalColor;
                yield return new WaitForSeconds(0.05f);
            }

            renderer.color = originalColor; // Ensure original color is restored
        }
    }

    //private IEnumerator HitFlash(GameObject target)
    //{
    //    SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
    //    if (renderer != null)
    //    {
    //        Material material = renderer.material;

    //        // Flash effect using _FlashAmount shader property
    //        if (material.HasProperty("_FlashAmount"))
    //        {
    //            material.SetFloat("_FlashAmount", 1);
    //            yield return new WaitForSeconds(0.05f);
    //            material.SetFloat("_FlashAmount", 0);
    //        }
    //    }
    //}


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
