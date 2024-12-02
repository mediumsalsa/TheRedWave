using Pathfinding;
using UnityEngine;

public abstract class Enemy : Entity
{
    public enum EnemyState { Patrolling, Chasing, Searching, Combat }

    [Header("Stat Settings")]
    public int maxHealth = 100;
    public int damage = 20;
    protected int health;

    [Header("Patrolling Settings")]
    public float patrolAreaRadius = 10f;
    public float patrolSpeed = 2f;
    protected Vector2 patrolAreaCenter;

    [Header("Chasing Settings")]
    public Transform target;
    public float detectionRange = 5f;
    public float chasingSpeed = 4f;

    [Header("Searching Settings")]
    public float searchDuration = 2f;
    public float searchWanderRadius = 3f;
    protected Vector3 lastKnownPosition;
    protected float searchTimer;

    [Header("Combat Settings")]
    public float combatStateRange = 1f;

    [Header("Vision Settings")]
    public LayerMask obstacleLayer;

    protected EnemyState currentState;
    protected AIDestinationSetter aiDestSet;
    protected AILerp aiLerp;
    protected ScreenEffects screenEffects;
    private SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        patrolAreaCenter = transform.position;
        currentState = EnemyState.Patrolling;

        aiDestSet = GetComponent<AIDestinationSetter>();
        aiLerp = GetComponent<AILerp>();
        screenEffects = FindObjectOfType<ScreenEffects>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        health = maxHealth;
        aiLerp.speed = patrolSpeed;
        SetRandomPatrolPoint();
    }

    protected virtual void Update()
    {
        if (isKnockedBack) return;

        HandleStateTransitions();
        HandleSpriteFlip();

        switch (currentState)
        {
            case EnemyState.Patrolling: PatrollingUpdate(); break;
            case EnemyState.Chasing: ChasingUpdate(); break;
            case EnemyState.Searching: SearchingUpdate(); break;
            case EnemyState.Combat: CombatUpdate(); break;
        }
    }

    // What else the child does when hit
    protected abstract void HandleOnHit(Collider2D other);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Hit") || other.transform.IsChildOf(transform)) return;

        EntityStats attacker = other.GetComponent<EntityStats>();
        if (attacker != null)
        {
            Vector2 knockbackDir = (transform.position - other.transform.position).normalized;
            isKnockedBack = true;
            StartCoroutine(screenEffects.ApplyKnockback(GetComponent<Rigidbody2D>(), knockbackDir, this));
            StartCoroutine(screenEffects.ApplyIframes(this, iFrameDuration, 0.3f));
            health -= attacker.damage;
        }

        HandleOnHit(other);
    }

    protected void HandleSpriteFlip()
    {
        // Use movement direction or velocity for sprite flipping
        Vector2 velocity = aiLerp.velocity; // AILerp's current velocity

        if (velocity.x > 0)
            spriteRenderer.flipX = false; // Facing right
        else if (velocity.x < 0)
            spriteRenderer.flipX = true; // Facing left
    }

    protected virtual void PatrollingUpdate()
    {
        if (aiLerp.reachedEndOfPath)
            SetRandomPatrolPoint();
    }

    protected virtual void ChasingUpdate()
    {
        aiDestSet.target = target;
        aiLerp.speed = chasingSpeed;
        lastKnownPosition = target.position;
    }

    protected virtual void SearchingUpdate()
    {
        searchTimer -= Time.deltaTime;
        if (aiLerp.reachedEndOfPath && searchTimer > 0)
        {
            Vector2 randomPoint = lastKnownPosition + (Vector3)(Random.insideUnitCircle * searchWanderRadius);
            SetTemporaryTarget(randomPoint);
        }
    }

    protected abstract void CombatUpdate();



    protected virtual void HandleStateTransitions()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        bool canSeePlayer = HasLineOfSightToPlayer();

        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (distanceToPlayer <= detectionRange && canSeePlayer)
                    currentState = EnemyState.Chasing;
                break;

            case EnemyState.Chasing:
                if (!canSeePlayer || distanceToPlayer > detectionRange)
                {
                    aiLerp.enabled = true;
                    currentState = EnemyState.Searching;
                    searchTimer = searchDuration;
                    SetTemporaryTarget(lastKnownPosition);
                }
                else if (distanceToPlayer <= combatStateRange)
                {
                    aiLerp.enabled = true;
                    currentState = EnemyState.Combat;
                }
                break;

            case EnemyState.Searching:
                if (distanceToPlayer <= detectionRange && canSeePlayer)
                    currentState = EnemyState.Chasing;
                else if (searchTimer <= 0)
                {
                    aiLerp.enabled = true;
                    currentState = EnemyState.Patrolling;
                    SetRandomPatrolPoint();
                }
                break;

            case EnemyState.Combat:
                if (!canSeePlayer || distanceToPlayer > detectionRange)
                {
                    aiLerp.enabled = true;
                    currentState = EnemyState.Searching;
                    searchTimer = searchDuration;
                    SetTemporaryTarget(lastKnownPosition);
                }
                break;
        }

        // Debug logs for testing
        Debug.Log($"Current State: {currentState}");
    }

    protected bool HasLineOfSightToPlayer()
    {
        Vector2 directionToPlayer = (target.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        return hit.collider == null;
    }

    protected void SetTemporaryTarget(Vector2 position)
    {
        GameObject tempTarget = new GameObject("SearchTarget");
        tempTarget.transform.position = new Vector3(position.x, position.y, 0);
        aiDestSet.target = tempTarget.transform;
        Destroy(tempTarget, 1f);
    }

    protected void SetRandomPatrolPoint()
    {
        Vector2 randomPoint = patrolAreaCenter + Random.insideUnitCircle * patrolAreaRadius;
        SetTemporaryTarget(randomPoint);
        aiLerp.speed = patrolSpeed;
    }
}