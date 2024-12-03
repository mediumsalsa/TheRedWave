using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : Entity
{
    [Header("Stats")]
    public int maxHealth;
    [SerializeField] private float movementSpeed = 1f, freezeDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private GameObject forwardHitbox, upHitbox, downHitbox;
    [SerializeField] private float iframeDur = 0.5f;
    [SerializeField] private float knockForce = 3f;
    [SerializeField] private float knockDuration = 0.1f;
    [SerializeField] private float flashDuration = 0.3f;

    private int health;
    private bool isAttacking = false;
    private Color originalColor;
    private Rigidbody2D rb;
    private Animator animator;
    private HealthSystem healthSystem;
    private ScreenEffects screenEffects;
    private SpriteRenderer spriteRenderer;
    private Material material;

    // Define hitbox offsets
    private Vector3 forwardHitboxOffset = new Vector3(0.1652f, -0.0256f, 0f);
    private Vector3 upHitboxOffset = new Vector3(0f, 0.1085f, 0f);
    private Vector3 downHitboxOffset = new Vector3(0f, -0.1816f, 0f);

    private void Start()
    {
        knockbackDuration = knockDuration;
        knockbackForce = knockForce;
        iFrameDuration = iframeDur;

        Time.timeScale = 1f;
        healthSystem = GetComponent<HealthSystem>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        material = spriteRenderer.material;
        screenEffects = FindObjectOfType<ScreenEffects>();

        health = maxHealth;
        originalColor = spriteRenderer.color;

        DeactivateHitboxes();
    }

    private void Update()
    {
        health = healthSystem.health;
        if (health <= 0) ResetLevel();
        if (isAttacking || isKnockedBack) return;

        HandleMovement();
        if (Input.GetMouseButtonDown(0)) Attack();
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    private void HandleMovement()
    {
        if (isKnockedBack) return;

        float hInput = Input.GetAxisRaw("Horizontal"), vInput = Input.GetAxisRaw("Vertical");
        animator.SetFloat("xInput", hInput);
        animator.SetFloat("yInput", vInput);

        animator.Play(hInput != 0 || vInput != 0 ? "Run" : "Idle");
        transform.localScale = new Vector3(hInput < 0 ? -1 : 1, 1, 1);

        Vector2 movement = new Vector2(hInput, vInput).normalized;
        rb.MovePosition(rb.position + movement * movementSpeed * Time.fixedDeltaTime);
    }

    private void Attack()
    {
        isAttacking = true;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;

        animator.SetFloat("xInput", direction.x);
        animator.SetFloat("yInput", direction.y);
        transform.localScale = new Vector3(mousePos.x < transform.position.x ? -1 : 1, 1, 1);

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            ActivateHitbox(forwardHitbox, forwardHitboxOffset);
        else
            ActivateHitbox(direction.y > 0 ? upHitbox : downHitbox,
                direction.y > 0 ? upHitboxOffset : downHitboxOffset);

        animator.Play("Attack");
    }

    private void ActivateHitbox(GameObject hitbox, Vector3 offset)
    {
        DeactivateHitboxes();

        if (hitbox != null)
        {
            // Reset the hitbox's parent and position
            if (hitbox.transform.parent != transform)
                hitbox.transform.SetParent(transform);

            hitbox.transform.localPosition = offset; // Apply the correct offset
            hitbox.SetActive(true);
        }
    }

    private void DeactivateHitboxes()
    {
        if (forwardHitbox) forwardHitbox.SetActive(false);
        if (upHitbox) upHitbox.SetActive(false);
        if (downHitbox) downHitbox.SetActive(false);
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        DeactivateHitboxes();
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
            StartCoroutine(screenEffects.ApplyKnockback(rb, knockbackDir, this));
            StartCoroutine(screenEffects.ApplyIframes(this, iframeDur, flashDuration));

            // Apply damage to health
            healthSystem.TakeDamage(attacker.gameObject);
        }
    }

    private void ResetLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
