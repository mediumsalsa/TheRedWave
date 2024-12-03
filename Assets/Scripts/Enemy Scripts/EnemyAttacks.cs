using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyAttacks : MonoBehaviour
{


    #region Dash Attack

    private enum SlimeCombatState { Idle, WindUp, Dashing, Recover }
    private SlimeCombatState combatState = SlimeCombatState.Idle;

    private float windUpDuration = 0.5f;
    private float dashSpeed = 8f;
    private float dashDuration = 0.3f;
    private float recoverDuration = 0.5f;

    private float stateTimer;
    private Vector2 dashDirection;
    private Rigidbody2D rb;
    private Animator animator; // Reference to Animator


    private void StartWindUp(GameObject gameObject)
    {
        combatState = SlimeCombatState.WindUp;
        stateTimer = windUpDuration;
        StopMovement();

        if (animator != null)
            animator.Play("WindUp"); // Play the wind-up animation

        Debug.Log("Slime is winding up!");
    }

    private void StartDash()
    {
        combatState = SlimeCombatState.Dashing;
        stateTimer = dashDuration;

        if (target != null)
            dashDirection = (target.position - transform.position).normalized;

        aiLerp.enabled = false;

        if (animator != null)
            animator.Play("JumpAttack"); // Play the jump attack animation

        Debug.Log("Slime is dashing!");
    }

    private void PerformDash()
    {
        rb.velocity = dashDirection * dashSpeed;

        // Stop animation on the last frame when dash is done
        if (stateTimer <= 0 && animator != null)
            animator.Play("JumpAttack"); // Freeze animation at the last frame
    }

    private void StartRecover()
    {
        combatState = SlimeCombatState.Recover;
        stateTimer = recoverDuration;
        animator.Play("Idle");
        StopMovement();
        Debug.Log("Slime is recovering!");
    }

    private void StopMovement()
    {
        rb.velocity = Vector2.zero;
        aiLerp.enabled = false;
    }

    private bool IsTargetInRange()
    {
        if (target == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        return distanceToTarget <= detectionRange && HasLineOfSightToTarget();
    }

    #endregion


}
