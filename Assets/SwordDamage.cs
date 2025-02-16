using System;
using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public float damageAmount = 25f;
    public AudioClip hitSound; // 🔊 Sound clip for hit effect
    private AudioSource audioSource;
    private PlayerAttackInput playerAttack;

    void Start()
    {
        // Get AudioSource component (must be attached to the sword)
        audioSource = GetComponent<AudioSource>();

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
                    Debug.Log("bullet hit");

                    // ✅ Play hit sound effect when damage is dealt
                    PlayHitSound();
                }
            }
        }
    }

    private void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
}


