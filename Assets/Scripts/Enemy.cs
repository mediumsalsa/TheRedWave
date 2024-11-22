using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public SpriteRenderer spriteRenderer;
    private ScreenEffects screenEffects;
    private AIDestinationSetter aiDestSet;
    private AILerp aiLerp;

    void Start()
    {
        screenEffects = FindObjectOfType<ScreenEffects>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        aiDestSet = GetComponent<AIDestinationSetter>();
        aiLerp = GetComponent<AILerp>();

        patrolAreaCenter = transform.position; 
        currentState = EnemyState.Patrolling;

        aiLerp.speed = patrolSpeed;
        SetRandomPatrolPoint();

        health = maxHealth;
        healthSystem = new HealthSystem(health);
        originalColor = spriteRenderer.color;
    }

    void Update()
    {
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (player != null && isKnockedBack == false)
            {
                //Apply damage
                healthSystem.TakeDamage(player.damage);
                health = healthSystem.health;
                Debug.Log("Enemy Health: " + health);
                StartCoroutine(HitFlash());
                Debug.Log("Before Hit Effects");
                StartCoroutine(HitEffects(collision.transform.position));
            }
        }
    }

    private IEnumerator HitEffects(Vector3 enemyPosition)
    {
        // Flash effect
        StartCoroutine(HitFlash());

        //Apply knockback
        Vector2 knockbackDirection = (transform.position - enemyPosition).normalized;
        StartCoroutine(ApplyKnockback(knockbackDirection));

        Debug.Log("After Knockback");

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
        aiLerp.enabled = false;
        aiDestSet.enabled = false; // Temporarily disable AI movement

        float timer = 0f;

        while (timer < knockbackDuration)
        {
            rb.MovePosition(rb.position + direction * knockbackForce * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        aiLerp.enabled = true; // Re-enable AI movement
        aiDestSet.enabled = true;
        isKnockedBack = false;
    }

}