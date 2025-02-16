using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public Slider EnemyHealthbar; // Reference to the UI health bar

    public GameObject[] lootPrefabs; // Assign different loot items in the Inspector
    public float dropChance = 0.5f; // 50% chance to drop loot

    private bool isDead = false; // Flag to track if the enemy has already died

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (EnemyHealthbar != null)
        {
            EnemyHealthbar.value = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        if (isDead) return; // Prevent multiple calls to Die()
        isDead = true; 

        Debug.Log(gameObject.name + " has died!");
        
        DropLoot(); // Drop loot before destroying

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(10);
            Debug.Log("Added 10 points!");
        }

        Destroy(gameObject);
    }

    void DropLoot()
    {
        if (lootPrefabs.Length > 0 && Random.value < dropChance)
        {
            int randomIndex = Random.Range(0, lootPrefabs.Length);
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f; // Drop slightly above
            Instantiate(lootPrefabs[randomIndex], dropPosition, Quaternion.Euler(90,0,0));
        }
    }
}