using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f; // Set the initial speed in the Inspector

    [HideInInspector]
    public Vector2 direction; 

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Apply the initial velocity based on speed and direction
            rb.velocity = direction * speed;
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized; // Normalize to ensure consistent speed
    }
}
