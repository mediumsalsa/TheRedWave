using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Slime : Enemy
{
    private enum SlimeCombatState { Idle, WindUp, Dashing, Recover }
    private SlimeCombatState combatState = SlimeCombatState.Idle;

    [Header("Dash Attack Settings")]
    public float windUpDuration = 0.5f;
    public float dashSpeed = 8f;
    public float dashDuration = 0.3f;
    public float recoverDuration = 0.5f;

    private float stateTimer;
    private Vector2 dashDirection;
    private Rigidbody2D rb;
    private Animator animator; // Reference to Animator

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Initialize Animator
    }

    protected override void HandleOnHit(Collider2D other)
    {
        if (health <= 0)
        {
            StopMovement();
            isDead = true;
            animator.Play("Die");
        }

        Debug.Log("Slime got hit!");
    }

    private void OnDeath()
    {
        Destroy(gameObject);
        Debug.Log("Slime died!");
    }

    protected override void CombatUpdate()
    {
        if (isDead) return;
        if (!IsTargetInRange()) // Check if the target is valid
        {
            ExitCombat();
            return;
        }

        stateTimer -= Time.deltaTime;

        switch (combatState)
        {
            case SlimeCombatState.Idle:
                if (currentState == EnemyState.Combat)
                {
                    StartWindUp();
                }
                break;
            case SlimeCombatState.WindUp:
                StopMovement();
                if (stateTimer <= 0) StartDash();
                break;
            case SlimeCombatState.Dashing:
                PerformDash();
                if (stateTimer <= 0) StartRecover();
                break;
            case SlimeCombatState.Recover:
                StopMovement();
                animator.Play("Idle");
                if (stateTimer <= 0) combatState = SlimeCombatState.Idle;
                break;
        }
    }

    private void StartWindUp()
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

    private void ExitCombat()
    {
        StopMovement();
        combatState = SlimeCombatState.Idle;
        aiLerp.enabled = true;
        currentState = EnemyState.Chasing;
        animator.Play("Idle");
        Debug.Log("Target left range, exiting combat.");
    }


}

