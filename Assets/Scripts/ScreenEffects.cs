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

    public void TriggerHitEffects(GameObject target, Vector3 enemyPosition)
    {
        StartCoroutine(HitEffectsRoutine(target, enemyPosition));
    }

    private IEnumerator HitEffectsRoutine(GameObject target, Vector3 enemyPosition)
    {
        // Flash effect
        StartCoroutine(HitFlash(target));

        // Knockback
        Vector2 knockbackDir = (target.transform.position - enemyPosition).normalized;

        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Entity entity = target.GetComponent<Entity>(); // Reference to the entity script
            if (entity != null)
            {
                StartCoroutine(ApplyKnockback(rb, knockbackDir, entity));
            }
        }

        // Freeze and shake
        FreezeFrame();
        yield return null;
    }

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

    private IEnumerator ApplyKnockback(Rigidbody2D rb, Vector2 direction, Entity entity)
    {
        if (rb == null)
        {
            Debug.LogWarning("Rigidbody2D is null. Knockback cancelled.");
            yield break;
        }

        entity.isKnockedBack = true;

        // Disable AI movement
        AILerp aiLerp = entity.GetComponent<AILerp>();
        if (aiLerp != null) aiLerp.enabled = false;

        float timer = 0f;

        while (timer < knockbackDuration)
        {
            if (rb == null)
            {
                Debug.LogWarning("Rigidbody2D was destroyed during knockback.");
                yield break;
            }

            rb.velocity = direction * knockbackForce;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Re-enable AI movement
        if (aiLerp != null) aiLerp.enabled = true;

        entity.isKnockedBack = false;
    }
}
