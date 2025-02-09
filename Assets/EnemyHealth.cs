using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public Slider EnemyHealthbar; // Reference to the UI health bar

    // Start is called once before the first execution of Update after the MonoBehaviour is created  
    void Start()
    {
        // Setting the CurrentHealth to MaxHealth, and calling the method UpdateHealthBar
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;  // the amount of damage reduce the enemies health 
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Logic to prevent negative health like -10

        UpdateHealthBar();
        
        if (currentHealth <= 0) // Lower than 0, and the enemies will die
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (EnemyHealthbar != null)   // Making sure the healthbar is full
        {
            EnemyHealthbar.value = currentHealth / maxHealth;
        }
    }

    private bool isDead = false; // Flag to track if the enemy has already died

void Die() // Handle player logic
{
    if (isDead) return; // ✅ Prevent multiple calls to Die()
    isDead = true; // ✅ Mark the enemy as dead

    Debug.Log(gameObject.name + " has died!");

    if (ScoreManager.Instance != null)
    {
        ScoreManager.Instance.AddScore(10);
        Debug.Log("Added 10 points!");
    }

    Destroy(gameObject);
}


}

