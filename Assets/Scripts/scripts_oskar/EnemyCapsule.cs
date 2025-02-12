using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCapsule : MonoBehaviour
{
    GameObject player;
    Transform player_transf;
    NavMeshAgent nma;
    // AgentLinkMover alm;

    void Start()
    {
        nma = GetComponent<NavMeshAgent>();
        nma.enabled = false;
        // alm = GetComponent<AgentLinkMover>();
    }

    void Update()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");
        if (player != null && player_transf == null)
            player_transf = player.GetComponent<Transform>();
        if (player == null || player_transf == null)
            return;

        if (player_transf.hasChanged) {
            if (Math.Sqrt(Vector3.Dot(player_transf.position - transform.position, player_transf.position - transform.position)) < 10) {
                // Debug.Log("starting to follow player from " + transform.position.ToString() + " to " + player_transf.position.ToString());
                nma.SetDestination(player_transf.position);
            }
        }
    }

    void OnCollisionEnter()
    {
        nma.enabled = true;
    }
}
