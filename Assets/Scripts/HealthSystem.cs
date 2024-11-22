using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class HealthSystem : MonoBehaviour
    {
        public int health { get; private set; } = 100; // Default health value

        private void Start()
        {
            health = 100; // Initialize with a meaningful default value
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
