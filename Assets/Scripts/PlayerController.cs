using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float movementSpeed = 1f;

    private Rigidbody2D rb;

    private Animator animator;

    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        if (isAttacking) return;

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

        // Trigger attack animation
        animator.Play("Attack");
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false; 
    }


}
