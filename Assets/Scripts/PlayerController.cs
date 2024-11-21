using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float movementSpeed = 1f;

    private Rigidbody2D rb;

    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        float hInput = Input.GetAxisRaw("Horizontal");
        float vInput = Input.GetAxisRaw("Vertical");

        if (hInput != 0 || vInput != 0)
        {
            animator.SetFloat("xInput", hInput);
            animator.SetFloat("yInput", vInput);
            animator.Play("Run");
        }
        if (hInput == 0 && vInput == 0)
        {
            animator.Play("Idle");
        }

        if (hInput > 0) 
        {
            transform.localScale = new Vector3(1, 1, 1); 
        }
        else if (hInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); 
        }

        Vector2 movement = new Vector2(hInput, vInput);
        movement.Normalize();

        rb.MovePosition(rb.position + movement * movementSpeed * Time.fixedDeltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }


}
