using System;
using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public float damageAmount = 25f;

    private PlayerAttackInput playerAttack;

    void Start()
    {
        // Finding PlayerAttackInput from parent (PlayerFree)
        playerAttack = GetComponentInParent<PlayerAttackInput>();

        if (playerAttack != null)
        {
            // Subscribing to the Attack event
            playerAttack.Attack += OnAttack;
        }
    }

    private void OnAttack()
    {
        // When the player attacks, it checks for enemies within the sword's hitbox.
        Collider[] hitEnemies = Physics.OverlapBox(transform.position, transform.localScale / 2);


        // For each enemy that is hit, it checks the tag and applies damage if it’s an enemy.
        foreach (Collider enemy in hitEnemies) 
        {
            if (enemy.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damageAmount);
                }
            }
        }
    }
}


