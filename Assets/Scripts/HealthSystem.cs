using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public int health { get; private set; }


    private void Start()
    {
        EntityStats myStats = GetComponent<EntityStats>();

        if (myStats == null) return;
        health = myStats.maxHealth;
    }

    public void TakeDamage(GameObject attacker)
    {
        EntityStats attackerStats = attacker.GetComponent<EntityStats>();

        if (attackerStats != null)
        {
            health -= attackerStats.damage;
            Debug.Log($"Health reduced to {health} by {attacker.name}");
        }
        else
        {
            Debug.LogWarning("Attacker does not have EntityStats attached!");
        }
           
    }

    public void Heal(int healAmount)
    {
        health += healAmount;
        Debug.Log($"Health increased to {health}");
    }
}
