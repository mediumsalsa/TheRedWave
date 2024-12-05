using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cocogrunt : Enemy
{
    private enum CombatState { Idle, WindUp, Dashing, Recover, RangedAttack, Strafing }
    private CombatState combatState = CombatState.Idle;

    [Header("Hit Settings")]
    public float knockDur;
    public float knockForce;

    [Header("Dash Attack Settings")]
    public GameObject explosionPrefab;
    public float windUpDuration = 0.5f;
    public float dashSpeed = 8f;
    public float dashDuration = 0.3f;
    public float recoverDuration = 0.5f;

    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;
    public float rangedAttackCooldown = 3f; // Increased cooldown for less frequent attacks
    public float rangedAttackRange = 10f;

    [Header("Strafe Settings")]
    public float strafeSpeed = 4f;
    public float strafeDuration = 1f; // How long to strafe

    private float stateTimer;
    private Vector2 dashDirection;
    private Rigidbody2D rb;
    private Animator animator;
    private float rangedAttackTimer;

    protected override void Start()
    {
        base.Start();

        knockbackDuration = knockDur;
        knockbackForce = knockForce;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rangedAttackTimer = 0f;
    }

    protected override void CombatUpdate()
    {
        if (isDead) return;
        if (!IsTargetInRange())
        {
            ExitCombat();
            return;
        }

        stateTimer -= Time.deltaTime;
        rangedAttackTimer -= Time.deltaTime;

        switch (combatState)
        {
            case CombatState.Idle:
                if (currentState == EnemyState.Combat)
                {
                    DecideNextAction();
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
            case CombatState.RangedAttack:
                StopMovement(); // Ensure no movement during attack
                if (stateTimer <= 0) combatState = CombatState.Idle;
                break;
            case CombatState.Strafing:
                PerformStrafe();
                if (stateTimer <= 0) combatState = CombatState.Idle;
                break;
        }
    }

    private void DecideNextAction()
    {
        if (rangedAttackTimer <= 0 && IsTargetInRangedAttackRange())
        {
            StartRangedAttack();
        }
        else if (Random.value > 0.5f) // 50% chance to strafe or dash back
        {
            StartStrafe();
        }
        else
        {
            StartWindUp();
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
        Explode();
        StopMovement();
    }

    private void StartRangedAttack()
    {
        combatState = CombatState.RangedAttack;
        stateTimer = 1f; // Duration of attack animation
        rangedAttackTimer = rangedAttackCooldown;

        if (animator != null)
            animator.Play("KickAttack");
    }

    // This method will be called via an Animation Event
    public void ThrowProjectile()
    {
        if (target != null && projectilePrefab != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.SetDirection(direction);
            }
        }
    }

    private void StartStrafe()
    {
        combatState = CombatState.Strafing;
        stateTimer = strafeDuration;

        // Choose a random strafe direction perpendicular to the target
        if (target != null)
        {
            Vector2 toTarget = (target.position - transform.position).normalized;
            Vector2 strafeDirection = new Vector2(-toTarget.y, toTarget.x); // Perpendicular direction
            rb.velocity = strafeDirection * strafeSpeed;
        }

        if (animator != null)
            animator.Play("Strafe");
    }

    private void PerformStrafe()
    {
        // Maintain velocity in the set direction
        // Strafe ends automatically when stateTimer <= 0
    }

    private bool IsTargetInRangedAttackRange()
    {
        if (target == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        return distanceToTarget <= rangedAttackRange && HasLineOfSightToTarget();
    }

    private void Explode()
    {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }

    private void ExitCombat()
    {
        StopMovement();
        combatState = CombatState.Idle;
        aiLerp.enabled = true;
        currentState = EnemyState.Chasing;
        animator.Play("Walk");
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
        Explode();
        Destroy(gameObject);
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
}




//protected override void HandleOnHit(Collider2D other)
//{
//    if (health <= 0)
//    {
//        StopMovement();
//        isDead = true;
//        //animator.Play("Die");
//    }
//}

//private void OnDeath()
//{
//    Explode();
//    Destroy(gameObject);
//}