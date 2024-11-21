using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private AIDestinationSetter aiDestSet;

    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Searching
    }

    private EnemyState currentState;

    [Header("Patrolling Settings")]
    private Vector2 patrolAreaCenter; 
    public float patrolAreaRadius = 10f; 
    public float patrolSpeed = 2f;

    [Header("Chasing Settings")]
    public Transform player; 
    public float detectionRange = 5f;
    public float chasingSpeed = 4f;

    [Header("Searching Settings")]
    public float searchDuration = 2f; 
    public float searchWanderRadius = 3f; 
    private Vector3 lastKnownPosition;
    private float searchTimer;

    [Header("Vision Settings")]
    public LayerMask obstacleLayer; 

    private AILerp aiLerp; 

    void Start()
    {
        aiDestSet = GetComponent<AIDestinationSetter>();
        aiLerp = GetComponent<AILerp>();

        patrolAreaCenter = transform.position; 
        currentState = EnemyState.Patrolling;

        aiLerp.speed = patrolSpeed; 
        SetRandomPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                PatrollingUpdate();
                break;

            case EnemyState.Chasing:
                ChasingUpdate();
                break;

            case EnemyState.Searching:
                SearchingUpdate();
                break;
        }

        HandleStateTransitions();
    }

    void PatrollingUpdate()
    {
        if (aiLerp.reachedEndOfPath)
        {
            SetRandomPatrolPoint();
        }
    }

    void ChasingUpdate()
    {
        aiDestSet.target = player; 
        aiLerp.speed = chasingSpeed; 
        lastKnownPosition = player.position; 
    }

    void SearchingUpdate()
    {
        searchTimer -= Time.deltaTime;

        if (aiLerp.reachedEndOfPath && searchTimer > 0)
        {
            Vector2 randomPoint = lastKnownPosition + (Vector3)(Random.insideUnitCircle * searchWanderRadius);
            SetTemporaryTarget(randomPoint);
        }
    }

    private void SetTemporaryTarget(Vector2 position)
    {
        GameObject tempTarget = new GameObject("SearchTarget");
        tempTarget.transform.position = new Vector3(position.x, position.y, 0);
        aiDestSet.target = tempTarget.transform;
        Destroy(tempTarget, 1f); 
    }

    void SetRandomPatrolPoint()
    {
        Vector2 randomPoint = patrolAreaCenter + Random.insideUnitCircle * patrolAreaRadius;
        SetTemporaryTarget(randomPoint);
        aiLerp.speed = patrolSpeed;
    }

    private bool HasLineOfSightToPlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        return hit.collider == null; 
    }

    void HandleStateTransitions()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = HasLineOfSightToPlayer();

        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (distanceToPlayer <= detectionRange && canSeePlayer)
                {
                    currentState = EnemyState.Chasing;
                }
                break;

            case EnemyState.Chasing:
                if (!canSeePlayer || distanceToPlayer > detectionRange)
                {
                    currentState = EnemyState.Searching;
                    searchTimer = searchDuration; 
                    SetTemporaryTarget(lastKnownPosition); 
                }
                break;

            case EnemyState.Searching:
                if (distanceToPlayer <= detectionRange && canSeePlayer)
                {
                    currentState = EnemyState.Chasing; 
                }
                else if (searchTimer <= 0)
                {
                    currentState = EnemyState.Patrolling;
                    SetRandomPatrolPoint(); 
                }
                break;
        }
    }

}