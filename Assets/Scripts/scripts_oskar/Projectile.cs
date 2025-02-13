using UnityEngine;

public class Projectile : MonoBehaviour
{
    GameObject game_controller;
    GameObject parent;

    void Awake()
    {
        parent = transform.parent.gameObject;
    }

    void Start()
    {

    }

    void Update()
    {

    }

    void OnCollisionEnter(Collision collision_data)
    {
        foreach (ContactPoint c_point in collision_data) {
            if (!c_point.otherCollider.gameObject.CompareTag("Player"))
                continue;
            c_point.otherCollider.gameObject.SendMessage("ProjectileHit", );
            game_controller.SendMessage("DamagePlayerEvent", 1);
        }
    }
}
