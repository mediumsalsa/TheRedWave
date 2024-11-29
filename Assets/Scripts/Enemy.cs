using Pathfinding;
using UnityEngine;

public class Enemy : Entity
{
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Searching
    }

    private EnemyState currentState;

    [Header("Stat Settings")]
    public int maxHealth = 100;
    public int damage = 20;
    private int health;

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

    [Header("Hit Settings")]
    [SerializeField] private float knockForce = 3f;
    [SerializeField] private float knockDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float iframeDuration = 0.5f;
    private Color originalColor;

    [Header("Vision Settings")]
    public LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private HealthSystem healthSystem;
    private SpriteRenderer spriteRenderer;
    private ScreenEffects screenEffects;
    private AIDestinationSetter aiDestSet;
    private AILerp aiLerp;

    void Start()
    {
        knockbackDuration = knockDuration;
        knockbackForce = knockForce;

        healthSystem = GetComponent<HealthSystem>();
        health = maxHealth;

        screenEffects = FindObjectOfType<ScreenEffects>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        aiDestSet = GetComponent<AIDestinationSetter>();
        aiLerp = GetComponent<AILerp>();

        originalColor = spriteRenderer.color;
        patrolAreaCenter = transform.position;
        currentState = EnemyState.Patrolling;

        aiLerp.speed = patrolSpeed;
        SetRandomPatrolPoint();
    }

    void Update()
    {
        if (isKnockedBack) return; // Prevent state updates during knockback

        health = healthSystem.health;
        if (health <= 0)
        {
            Destroy(gameObject);
            return;
        }

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
        HandleSpriteFlip();
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

    private bool HasLineOfSightToPlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        return hit.collider == null;
    }

    void HandleSpriteFlip()
    {
        spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Hit") || other.transform.IsChildOf(transform)) return;

        EntityStats attacker = other.GetComponent<EntityStats>();
        if (attacker != null)
        {
            // Calculate knockback direction
            Vector2 knockbackDir = (transform.position - other.transform.position).normalized;

            // Apply knockback and iframes
            isKnockedBack = true;
            StartCoroutine(screenEffects.ApplyKnockback(rb, knockbackDir, this));
            StartCoroutine(screenEffects.ApplyIframes(this, iframeDuration, flashColor));

            // Apply damage to health
            healthSystem.TakeDamage(attacker.gameObject);
        }
    }
}