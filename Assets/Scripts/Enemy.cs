using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;


public class Enemy : MonoBehaviour
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
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float freezeDuration = 0.1f;
    private bool isKnockedBack = false;
    private Color originalColor;

    [Header("Vision Settings")]
    public LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private HealthSystem healthSystem;
    private SpriteRenderer spriteRenderer;
    private ScreenEffects screenEffects;
    private AIDestinationSetter aiDestSet;
    private AILerp aiLerp;
    private Animator animator;
    private Vector2 previousPosition;

    private float xInput;
    private float yInput;
    private float speed;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        health = maxHealth;
        
        if (healthSystem == null) Debug.Log("HealthSystem object not found!");

        screenEffects = FindObjectOfType<ScreenEffects>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        aiDestSet = GetComponent<AIDestinationSetter>();
        aiLerp = GetComponent<AILerp>();

        patrolAreaCenter = transform.position;
        currentState = EnemyState.Patrolling;

        aiLerp.speed = patrolSpeed;
        SetRandomPatrolPoint();

        isKnockedBack = false;
        health = maxHealth;
        originalColor = spriteRenderer.color;

        previousPosition = transform.position;
    }

    void Update()
    {
        health = healthSystem.health;
        if (health <= 0)
        {
            Destroy(gameObject);
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
        UpdateAnimationParameters();
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

    void HandleSpriteFlip()
    {
        Vector2 currentPosition = transform.position;
        Vector2 direction = currentPosition - previousPosition;

        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        previousPosition = currentPosition;
    }

    void UpdateAnimationParameters()
    {
        Vector2 velocity = aiLerp.velocity;

        // Update Animator parameters
        animator.SetFloat("xInput", velocity.x);
        animator.SetFloat("yInput", velocity.y);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Hit"))
            return;

        // Ensure the hitbox is not part of this object (self-hit protection)
        if (other.transform.IsChildOf(transform))
            return;

        // Get the attacker's stats if applicable
        EntityStats attackerStats = other.GetComponent<EntityStats>();
        if (attackerStats != null && !isKnockedBack)
        {
            Debug.Log($"{gameObject.name} had a collision Detected with {other.name}, Tag: {other.tag}, IsChild: {other.transform.IsChildOf(transform)}");
            // Apply damage
            healthSystem.TakeDamage(attackerStats.gameObject);
            StartCoroutine(HitFlash());
            StartCoroutine(HitEffects(other.transform.position));
        }
    }

    private IEnumerator HitEffects(Vector3 enemyPosition)
    {
        // Flash effect
        StartCoroutine(HitFlash());

        //Apply knockback
        screenEffects.FreezeFrame();
        Vector2 knockbackDirection = (transform.position - enemyPosition).normalized;
        StartCoroutine(ApplyKnockback(knockbackDirection));
        yield return null;
    }

    private IEnumerator HitFlash()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;

        // Temporarily disable AILerp movement
        AILerp aiLerp = GetComponent<AILerp>();
        if (aiLerp != null)
            aiLerp.enabled = false;

        float timer = 0f;
        while (timer < knockbackDuration)
        {
            rb.MovePosition(rb.position + direction * knockbackForce * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (aiLerp != null)
            aiLerp.enabled = true;

        isKnockedBack = false;
    }

}