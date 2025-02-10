using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Animator doorAnimator; // Assign the Animator in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure it's the player triggering it
        {
            Debug.Log("🚪 Door is opening!");
            doorAnimator.Play("WallDoorAnimation"); // Play animation directly
        }
    }
}



