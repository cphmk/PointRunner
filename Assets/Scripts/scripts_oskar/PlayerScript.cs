using System;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GameObject projectile_prefab;

    GameObject game_controller;

    public float shoot_cooldown = 0.5f;
    float shoot_cooldown_left = 0;
    public int hp = 100;
    public int hp_max = 100;
    public int damage = 1;
    public int collision_damage = 1;
    public int projectile_speed = 2000;
    public float projectile_scale = 0.1f;
    public float projectile_duration = 1;

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
                    ProjectileScript props = proj.GetComponent<ProjectileScript>();

                    props.damage = damage;
                    props.speed = projectile_speed;
                    props.duration = projectile_duration;
                    props.scale = projectile_scale;

                    shoot_cooldown_left = shoot_cooldown;
            }
        }
        shoot_cooldown_left -= Time.deltaTime;
        shoot_cooldown_left = Math.Max(0, shoot_cooldown_left);
    }

    void OnProjectileHit(int damage)
    {
        // Debug.Log("OnProjectileHit: " + damage);
        hp -= damage;
        game_controller.SendMessage("DamagePlayer", damage);
    }

    void OnEnemyCollision(int damage)
    {
        // Debug.Log("OnEnemyCollision: " + damage);
        hp -= damage;
        game_controller.SendMessage("DamagePlayer", damage);
    }
}
