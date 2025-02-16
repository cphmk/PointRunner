using System;
using UnityEngine;
using TMPro;

public class PlayerScript : MonoBehaviour
{
    public GameObject projectile_prefab;
    public Color status_text_color = Color.green;
    public GameObject bar_prefab;

    TextMeshProUGUI status_text;
    GameObject game_controller;
    Camera cam;

    public float shoot_cooldown = 0.5f;
    float shoot_cooldown_left = 0;
    public int hp = 100;
    public int hp_max = 100;
    public int damage = 1;
    public int collision_damage = 1;
    public int projectile_speed = 2000;
    public float projectile_scale = 0.1f;
    public float projectile_duration = 1;
    float status_text_fade_time = 1;
    float status_text_fade = 0;

    void Awake()
    {
    }

    void Start()
    {
        status_text = GameObject.Find("PlayerUpdateText").GetComponent<TextMeshProUGUI>();
        status_text.text = "";
        cam = GetComponent<FirstPersonController>().playerCamera;
        game_controller = transform.parent.gameObject;
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) {
            if (shoot_cooldown_left == 0) {
                Ray aim_ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                Vector3 aim_pos = new Vector3(transform.position.x + aim_ray.direction.x,
                        transform.position.y + aim_ray.direction.y + transform.localScale.y / 4,
                        transform.position.z + aim_ray.direction.z);

                GameObject proj = Instantiate(projectile_prefab, aim_pos,
                        Quaternion.FromToRotation(Vector3.forward, aim_ray.direction),
                        game_controller.transform);
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

        if (status_text_fade > 0) {
            status_text_fade -= Time.deltaTime;
            status_text.color = status_text_color - new Color(0, 0, 0, (status_text_fade_time - status_text_fade) / status_text_fade_time);
        }
        else
            status_text.color = status_text_color - new Color(0, 0, 0, 1);
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

    void OnReward(Reward reward)
    {
        game_controller.SendMessage("RewardPlayer");
        hp += reward.hp;
        damage += reward.damage;
        hp_max += reward.hp_max;
        projectile_speed += reward.projectile_speed;
        projectile_scale += reward.projectile_scale;
        projectile_duration += reward.projectile_duration;
        if (reward.shoot_cooldown != 0)
            shoot_cooldown -= shoot_cooldown / 10.0f;

        string reward_str = "";
        if (reward.hp > 0)
            reward_str = "+" + reward.hp.ToString() + "HP";
        if (reward.damage > 0)
            reward_str = "+" + reward.damage.ToString() + "DAMAGE";
        if (reward.hp_max > 0)
            reward_str = "+" + reward.hp_max.ToString() + "MAX HP";
        if (reward.projectile_speed > 0)
            reward_str = "+" + reward.projectile_speed.ToString() + "PROJ SPEED";
        if (reward.projectile_scale > 0)
            reward_str = "+" + reward.projectile_scale.ToString() + "PROJ SCALE";
        if (reward.projectile_duration > 0)
            reward_str = "+" + reward.projectile_duration.ToString() + "PROJ DURATION";
        if (reward.shoot_cooldown > 0)
            reward_str = "-" + (shoot_cooldown / 20.0f).ToString() + "RELOAD TIME";

        status_text.text = reward_str;
        status_text.color = status_text_color;
        status_text_fade = status_text_fade_time;

        // Debug.Log(reward_str);
    }
}
