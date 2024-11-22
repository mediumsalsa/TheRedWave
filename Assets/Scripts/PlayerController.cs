using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Stat Settings")]
    public int maxHealth;
    public int damage;
    [SerializeField] private float movementSpeed = 1f;
    private int health;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float freezeDuration = 0.1f;
    private bool isKnockedBack = false;
    private Color originalColor;

    [Header("Attack Hitbox")]
    [SerializeField] private GameObject forwardHitbox;
    [SerializeField] private GameObject upHitbox;
    [SerializeField] private GameObject downHitbox;

    private Rigidbody2D rb;
    private Animator animator;
    private HealthSystem healthSystem;
    public SpriteRenderer spriteRenderer;
    private ScreenEffects screenEffects;

    private bool isAttacking = false;


    void Start()
    {
        Time.timeScale = 1f;

        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null) Debug.Log("HealthSystem object not found!");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D component is missing!");

        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator component is missing!");

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is missing!");
            return; //Exit to avoid further errors
        }

        health = maxHealth;
        originalColor = spriteRenderer.color;

        screenEffects = FindObjectOfType<ScreenEffects>();

        if (forwardHitbox != null)
        {
            forwardHitbox.SetActive(false);
        }
        if (upHitbox != null)
        {
            upHitbox.SetActive(false);
        }

        if (downHitbox != null)
        {
            downHitbox.SetActive(false);
        }
    }


    void Update()
    {
        health = healthSystem.health;
        if (health <= 0)
        {
            ResetLevel();
        }

        if (isAttacking || isKnockedBack) return;

        float hInput = Input.GetAxisRaw("Horizontal");
        float vInput = Input.GetAxisRaw("Vertical");

        //Moving
        if (hInput != 0 || vInput != 0)
        {
            animator.SetFloat("xInput", hInput);
            animator.SetFloat("yInput", vInput);
            animator.Play("Run");
        }

        //Not moving
        if (hInput == 0 && vInput == 0)
        {
            animator.Play("Idle");
        }

        //Flip the players sprites based on direction
        if (hInput > 0) 
        {
            transform.localScale = new Vector3(1, 1, 1); 
        }
        else if (hInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); 
        }

        //Player Attacks
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        Vector2 movement = new Vector2(hInput, vInput);
        movement.Normalize();

        rb.MovePosition(rb.position + movement * movementSpeed * Time.fixedDeltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }

    private void Attack()
    {
        isAttacking = true;

        // Get mouse position in world coordinates
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - transform.position).normalized;

        // Determine the attack direction
        float xDir = direction.x;
        float yDir = direction.y;

        // Set animator parameters for attack direction
        animator.SetFloat("xInput", xDir);
        animator.SetFloat("yInput", yDir);

        // Flip sprite based on mouse position relative to player
        if (mousePosition.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Face left
        }
        else if (mousePosition.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1); // Face right
        }

        // Activate the appropriate hitbox
        if (Mathf.Abs(xDir) > Mathf.Abs(yDir))
        {
            // Forward attack
            ActivateHitbox(forwardHitbox);
        }
        else if (yDir > 0)
        {
            // Upward attack
            ActivateHitbox(upHitbox);
        }
        else
        {
            // Downward attack
            ActivateHitbox(downHitbox);
        }

        // Trigger attack animation
        animator.Play("Attack");
    }

    private void ActivateHitbox(GameObject hitbox)
    {
        // Disable all hitboxes first
        if (forwardHitbox != null) forwardHitbox.SetActive(false);
        if (upHitbox != null) upHitbox.SetActive(false);
        if (downHitbox != null) downHitbox.SetActive(false);

        // Enable the specified hitbox
        if (hitbox != null) hitbox.SetActive(true);
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;

        // Disable all hitboxes
        if (forwardHitbox != null) forwardHitbox.SetActive(false);
        if (upHitbox != null) upHitbox.SetActive(false);
        if (downHitbox != null) downHitbox.SetActive(false);
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
        Vector2 knockbackDirection = (transform.position - enemyPosition).normalized;
        StartCoroutine(ApplyKnockback(knockbackDirection));

        //Trigger screen shake and freeze frame
        if (screenEffects != null)
        {
            screenEffects.FreezeFrame();
            screenEffects.ScreenShake();
        }
        else
        {
            Debug.LogError("ScreenEffects instance is missing!");
        }

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


    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
