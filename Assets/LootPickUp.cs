using UnityEngine;

public class LootPickup : MonoBehaviour
{
    public string playerTag = "Player"; // Ensure your player has this tag

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            CollectLoot();
        }
    }

    void CollectLoot()
    {
        Debug.Log("Loot Collected!");
        Destroy(gameObject); // Remove loot
    }
}