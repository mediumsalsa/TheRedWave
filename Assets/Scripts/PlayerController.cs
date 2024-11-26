using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth;
    [SerializeField] private float movementSpeed = 1f, knockbackForce = 5f, knockbackDuration = 0.5f, freezeDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private GameObject forwardHitbox, upHitbox, downHitbox;

    private int health;
    private bool isKnockedBack = false, isAttacking = false;
    private Color originalColor;
    private Rigidbody2D rb;
    private Animator animator;
    private HealthSystem healthSystem;
    private ScreenEffects screenEffects;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        Time.timeScale = 1f;
        healthSystem = GetComponent<HealthSystem>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            ActivateHitbox(forwardHitbox);
        else
            ActivateHitbox(direction.y > 0 ? upHitbox : downHitbox);

        animator.Play("Attack");
    }

    private void ActivateHitbox(GameObject hitbox)
    {
        DeactivateHitboxes();
        if (hitbox != null) hitbox.SetActive(true);
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
        if (attacker != null && !isKnockedBack)
        {
            healthSystem.TakeDamage(attacker.gameObject);
            StartCoroutine(HitEffects(other.transform.position));
        }
    }

    private IEnumerator HitEffects(Vector3 enemyPosition)
    {
        StartCoroutine(HitFlash());
        Vector2 knockbackDir = (transform.position - enemyPosition).normalized;
        StartCoroutine(ApplyKnockback(knockbackDir));

        screenEffects?.FreezeFrame();
        screenEffects?.ScreenShake();
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
        float timer = 0f;
        while (timer < knockbackDuration)
        {
            rb.MovePosition(rb.position + direction * knockbackForce * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isKnockedBack = false;
    }

    private void ResetLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
