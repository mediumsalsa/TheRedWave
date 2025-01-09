using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ReefTitan : Enemy
{
    private enum TitanCombatState { Idle, Shell, }
    private TitanCombatState combatState = TitanCombatState.Idle;

    private float stateTimer;
    private Vector2 dashDirection;
    private Rigidbody2D rb;
    private Animator animator; 

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
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
        switch (combatState)
        {
            case TitanCombatState.Idle:
                animator.Play("Idle");
                Debug.Log("State = Idle");
                if (currentState == EnemyState.Patrolling)
                {
                    combatState = TitanCombatState.Shell;
                }
                break;
            case TitanCombatState.Shell:
                animator.Play("State = Shell");
                if (currentState != EnemyState.Patrolling)
                {
                    combatState = TitanCombatState.Idle;
                }
                break;
        }
    }

    private void StopMovement()
    {
        rb.velocity = Vector2.zero;
        aiLerp.enabled = false;
    }

}
