using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject projectile_prefab;

    GameObject game_controller;

    float shoot_cooldown = 0.5f;
    float shoot_cooldown_left = 0;
    float proj_speed = 2000;

    void Awake()
    {
        game_controller = transform.parent.gameObject;
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButton(0)) {
            if (shoot_cooldown_left == 0) {
                GameObject proj = Instantiate(projectile_prefab, transform.position + transform.forward,
                        Quaternion.FromToRotation(Vector3.forward, transform.forward), game_controller.transform);
                proj.GetComponent<Rigidbody>().AddForce(transform.forward * proj_speed);
                Destroy(proj, 1);
                shoot_cooldown_left = shoot_cooldown;
            }
            else {
                shoot_cooldown_left -= Time.deltaTime;
                shoot_cooldown_left = Math.Max(0, shoot_cooldown_left);
            }
        }
    }
}
