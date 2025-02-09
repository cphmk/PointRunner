using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 200f;
    private float currentHealth;

    public Slider PlayerHealthbar; // Reference to the player's UI health bar


    // Start is called once before the first execution of Update after the MonoBehaviour is created  
    void Start()
    {
        // Setting the CurrentHealth to MaxHealth, and calling the method UpdateHealthBar
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        
        currentHealth -= damageAmount; // the amount of damage reduce the enemies health 
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Logic to prevent negative health like -10

        UpdateHealthBar();

        if (currentHealth <= 0) // Lower than 0, and the player will die
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (PlayerHealthbar != null) // Making sure the healthbar is full
        {
            PlayerHealthbar.value = currentHealth / maxHealth;
        }
    }

   void Die()
   { 
    Debug.Log("Player has died!");

    // Detaching the PlayerCamera before destroying the player
    Transform playerCamera = transform.Find("PlayerCamera");
    if (playerCamera != null)
    {
        playerCamera.SetParent(null); // Detaching the camera
    }

     Destroy(gameObject); // Now it's safe to destroy the player
   }

}

