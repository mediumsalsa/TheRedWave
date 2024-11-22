using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{

    public int health { get; private set; }

    public HealthSystem(int health)
    {
        this.health = health;
    }

    public void TakeDamage(int hit)
    {
        health -= hit;
    }

    public void heal(int heal)
    {
        health += heal;
    }

}
