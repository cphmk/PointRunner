using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public GameObject metalSword; // Reference to the sword GameObject
    public GameObject weapon02;   // Reference to the gun GameObject


    private void Start()
    {
        // Ensure no weapon is active at the start
        UnequipAll();
    }

    private void Update()
    {
        // Check for weapon equip
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Press '1'
        {
            EquipSword();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // Press '2'
        {
            EquipGun();
        }
        
    }

    private void EquipSword()
    {
        // Enable the sword and disable the gun
        metalSword.SetActive(true);
        weapon02.SetActive(false);
    }

    private void EquipGun()
    {
        // Enable the gun and disable the sword
        weapon02.SetActive(true);
        metalSword.SetActive(false);
    }

    private void UnequipAll()
    {
        // Disable both weapons
        metalSword.SetActive(false);
        weapon02.SetActive(false);
    }
}
