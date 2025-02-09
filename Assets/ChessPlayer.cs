using UnityEngine;
using UnityEngine.AI;

public class ChessPlayer : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent agent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created  
    void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player"); // Safely find the player object
        if (playerObject != null)
        {
            player = playerObject.transform; // Assign transform only if player exists
        }
        agent = GetComponent<NavMeshAgent>(); // Getting the NavMeshAgent component
    }

    void Update()
    {
        if (player != null) // Check if the player still exists before setting destination
        {
            agent.SetDestination(player.position);
        }
    }
}