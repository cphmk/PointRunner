using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCapsuleScript : MonoBehaviour
{
    public GameObject projectile_prefab;

    GameObject game_controller;
    GameObject player;
    Transform player_transf;
    NavMeshAgent nma;
    NavMeshPath path_to_player;
    Rigidbody rb;
    Material material;
    Color color_orig;

    /* game logic*/
    public float aggro_range = 15.0f;
    public bool landed = false;
    public float shoot_cooldown = 0.5f;
    public float shoot_cooldown_left = 0;
    public int hp = 1;
    public int damage = 1;
    public int collision_damage = 1;
    public int projectile_speed = 1000;
    public float projectile_scale = 0.1f;
    public float projectile_duration = 1;
    /* animation */
    float death_anim_total = 0.5f;
    float death_anim_secs = 0;
    float damage_anim_remaining_secs = 0;

    void Awake()
    {
        game_controller = transform.parent.gameObject;
        path_to_player = new NavMeshPath();
        rb = GetComponent<Rigidbody>();
        nma = GetComponent<NavMeshAgent>();
        nma.enabled = false;
        material = GetComponent<Renderer>().material;
        color_orig = material.color;
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

        Color color_temp = color_orig;

        if (death_anim_secs > 0) {
            death_anim_secs -= Time.deltaTime;
            color_temp.r = color_temp.r - (death_anim_total - death_anim_secs);
            color_temp.g = color_temp.g - (death_anim_total - death_anim_secs);
            color_temp.b = color_temp.b - (death_anim_total - death_anim_secs);
        }

        else if (damage_anim_remaining_secs > 0) {
            damage_anim_remaining_secs -= Time.deltaTime;
            color_temp.r = (float)Math.Abs(Math.Sin(Time.time * 10));
        }

        material.color = color_temp;

        if (landed && player.activeInHierarchy) {
            Vector3 diff_vec = player_transf.position - transform.position;
            float euclidean_dist = (float)Math.Sqrt(Vector3.Dot(diff_vec, diff_vec));
            if (euclidean_dist > aggro_range)
                return;

            try {
                nma.CalculatePath(player_transf.position, path_to_player);

                float d = DistToPlayer();
                if (d <= aggro_range && d > 0.001f) {
                    nma.SetDestination(player_transf.position);
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
            catch (Exception e) {

            }
        }

    }

    void OnCollisionEnter(Collision collision_data)
    {
        foreach (ContactPoint c_point in collision_data) {
            if (!landed && collision_data.gameObject.name == "Floor") {
                nma.enabled = true;
                landed = true;
            }
            c_point.otherCollider.gameObject.SendMessage("OnEnemyCollision", damage, SendMessageOptions.DontRequireReceiver);
            // rb.AddForceAtPosition(c_point.normal * 100, c_point.point);
        }
        // rb.linearVelocity = new Vector3(0, 0, 0);
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
        // Debug.Log("EnemyCapsule OnProjectileHit: " + damage);
        hp -= damage;
        if (hp < 1)
            KillThis();
        DamageThis(damage);
    }

    void DamageThis(int d)
    {
        damage_anim_remaining_secs = 0.25f * (float)Math.Sqrt(d);
    }

    void KillThis()
    {
        death_anim_secs = death_anim_total;
        Destroy(gameObject, death_anim_total);
    }
}
