using UnityEngine;

public class StaticCamera : MonoBehaviour
{
    public Transform player; // Assign your player in the Inspector
    private Vector3 offset;  // The fixed offset from the player

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player transform not assigned to camera script!");
            return;
        }

        // Store the initial offset, but only for X and Z (ignoring Y)
        offset = new Vector3(
            transform.position.x - player.position.x,
            transform.position.y,  // Lock the Y position
            transform.position.z - player.position.z
        );
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Maintain X and Z offset, but keep the original Y position
            transform.position = new Vector3(
                player.position.x + offset.x,  // Keep horizontal offset
                offset.y,                      // Lock Y position (no jumping effect)
                player.position.z + offset.z   // Keep depth offset
            );
        }
    }
}