using UnityEngine;

public class GoalScript : MonoBehaviour
{
    GameObject game_controller;
    GameObject player;

    void Start()
    {
        game_controller = transform.parent.parent.gameObject;
    }

    void OnCollisionEnter(Collision collision_data)
    {
        Debug.Log("OnCollisionEnter");
        if (collision_data.gameObject.CompareTag("Player"))
            game_controller.SendMessage("OnGoal");
    }
}
