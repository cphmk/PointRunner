using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCapsule : MonoBehaviour
{
    public GameObject projectile_prefab;

    GameObject game_controller;
    GameObject player;
    Transform player_transf;
    NavMeshAgent nma;
    NavMeshPath path_to_player;

    /* game logic*/
    public float aggro_range = 15.0f;
    public bool landed = false;
    public float shoot_cooldown = 0.5f;
    public float shoot_cooldown_left = 0;
    public int damage = 1;
    public int collision_damage = 1;
    public int projectile_speed = 1000;
    public float projectile_scale = 0.1f;
    public float projectile_duration = 1;

    void Awake()
    {
        game_controller = transform.parent.gameObject;
        path_to_player = new NavMeshPath();
        nma = GetComponent<NavMeshAgent>();
        nma.enabled = false;
    }

    void Start()
    {
    }

    void Update()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");
        if (player != null && player_transf == null)
            player_transf = player.GetComponent<Transform>();
        if (player == null || player_transf == null)
            return;

        if (landed) {
            nma.CalculatePath(player_transf.position, path_to_player);

            float d = DistToPlayer();
            if (d <= aggro_range && d > 0.001f) {
                nma.SetDestination(player_transf.position);
                Vector3 diff_vec = player_transf.position - transform.position;
                Vector3 diff_vec_norm = Vector3.Normalize(diff_vec);

                if (shoot_cooldown_left == 0) {
                    GameObject proj = Instantiate(projectile_prefab, transform.position + diff_vec_norm,
                            Quaternion.FromToRotation(Vector3.forward, diff_vec_norm), game_controller.transform);
                    ProjectileScript props = proj.GetComponent<ProjectileScript>();

                    props.damage = damage;
                    props.speed = projectile_speed;
                    props.duration = projectile_duration;
                    props.scale = projectile_scale;

                    shoot_cooldown_left = shoot_cooldown;
                }
                else {
                    shoot_cooldown_left -= Time.deltaTime;
                    shoot_cooldown_left = Math.Max(0, shoot_cooldown_left);
                }
            }
            else {
                nma.ResetPath();
            }
        }
    }

    void OnCollisionEnter(Collision collision_data)
    {
        if (!landed) {
            nma.enabled = true;
            landed = true;
        }

        foreach (ContactPoint c_point in collision_data) {
            game_controller.SendMessage("OnEnemyCollision", collision_damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    float DistToPlayer()
    {
        float d = 0.0f;
        for (int i = 1; i < path_to_player.corners.Length; ++i)
            d += Vector3.Distance(path_to_player.corners[i - 1], path_to_player.corners[i]);
        return d;
    }

    void OnProjectileHit(int damage)
    {
        Debug.Log("OnProjectileHit: " + damage);
    }
}
