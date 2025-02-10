using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public float damageAmount = 10f; // Damage dealt to the player per collision
    public float damageCooldown = 1f; // Cooldown between damage instances
    private bool canDamage = true;    // To prevent continuous damage spam


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canDamage)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null) // Checking if the player health component exists
            {
                playerHealth.TakeDamage(damageAmount);
                StartCoroutine(DamageCooldown());
            }
        }
    }

    private System.Collections.IEnumerator DamageCooldown()
    {
        canDamage = false;                   // Disable damage temporarily
        yield return new WaitForSeconds(damageCooldown); // Waiting for cooldown
        canDamage = true;                    // Enable damage again
    }
}

