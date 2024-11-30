using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cocogrunt : Enemy
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void HandleOnHit(Collider2D other)
    {
        if (health <= 0) Destroy(gameObject);
        Debug.Log("Cocogrunt got hit!");
    }

    protected override void CombatUpdate()
    {

    }
}
