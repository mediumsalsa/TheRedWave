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

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void HandleOnHit(Collider2D other)
    {
        if (health <= 0) Destroy(gameObject);
        Debug.Log("Slime got hit!");
    }

    protected override void CombatUpdate()
    {
        if (!IsTargetInRange()) // Check if the target is valid
        {
            ExitCombat();
            return;
        }

        stateTimer -= Time.deltaTime;

        switch (combatState)
        {
            case SlimeCombatState.Idle:
                StartWindUp();
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
                if (stateTimer <= 0) combatState = SlimeCombatState.Idle;
                break;
        }
    }

    private void StartWindUp()
    {
        combatState = SlimeCombatState.WindUp;
        stateTimer = windUpDuration;
        StopMovement();
        Debug.Log("Slime is winding up!");
    }

    private void StartDash()
    {
        combatState = SlimeCombatState.Dashing;
        stateTimer = dashDuration;

        if (target != null)
            dashDirection = (target.position - transform.position).normalized;

        aiLerp.enabled = false;
        Debug.Log("Slime is dashing!");
    }

    private void PerformDash()
    {
        rb.velocity = dashDirection * dashSpeed;
    }

    private void StartRecover()
    {
        combatState = SlimeCombatState.Recover;
        stateTimer = recoverDuration;
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
        Debug.Log("Target left range, exiting combat.");
    }

    private bool HasLineOfSightToTarget()
    {
        if (target == null) return false;

        Vector2 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer);
        return hit.collider == null; // No obstacle in the way
    }
}
