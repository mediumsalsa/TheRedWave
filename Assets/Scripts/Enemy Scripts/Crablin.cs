using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crablin : Enemy
{
    private enum CombatState { Idle, WindUp, Dashing, Recover }
    private CombatState combatState = CombatState.Idle;

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
    }

    private void OnDeath()
    {
        Destroy(gameObject);
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
            case CombatState.Idle:
                if (currentState == EnemyState.Combat)
                {
                    StartWindUp();
                }
                break;
            case CombatState.WindUp:
                StopMovement();
                if (stateTimer <= 0) StartDash();
                break;
            case CombatState.Dashing:
                PerformDash();
                if (stateTimer <= 0) StartRecover();
                break;
            case CombatState.Recover:
                StopMovement();
                animator.Play("Walk");
                if (stateTimer <= 0) combatState = CombatState.Idle;
                break;
        }
    }

    private void StartWindUp()
    {
        combatState = CombatState.WindUp;
        stateTimer = windUpDuration;
        StopMovement();

        if (animator != null)
            animator.Play("WindUp");
    }

    private void StartDash()
    {
        combatState = CombatState.Dashing;
        stateTimer = dashDuration;

        if (target != null)
            dashDirection = (target.position - transform.position).normalized;

        aiLerp.enabled = false;

        if (animator != null)
            animator.Play("JumpAttack"); 
    }

    private void PerformDash()
    {
        rb.velocity = dashDirection * dashSpeed;

        if (stateTimer <= 0 && animator != null)
            animator.Play("JumpAttack"); 
    }

    private void StartRecover()
    {
        combatState = CombatState.Recover;
        stateTimer = recoverDuration;
        animator.Play("Walk");
        StopMovement();
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
        combatState = CombatState.Idle;
        aiLerp.enabled = true;
        currentState = EnemyState.Chasing;
        animator.Play("Walk");
        Debug.Log("Target left range, exiting combat.");
    }


}
