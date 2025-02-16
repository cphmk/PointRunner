using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCapsuleScript : MonoBehaviour
{
    public GameObject projectile_prefab;
    public GameObject reward_prefab;
    public GameObject bar_prefab;

    GameObject game_controller;
    GameObject player;
    Transform player_transf;
    NavMeshAgent nma;
    NavMeshPath path_to_player;
    Rigidbody rb;
    Material material;
    Color color_orig;
    Collider collider;
    System.Random rng = new System.Random();

    GameObject bar_hp_max;
    GameObject bar_hp_cur;

    /* game logic*/
    public float drop_chance = 1;
    public float aggro_range = 15.0f;
    public bool landed = false;
    public float shoot_cooldown = 0.5f;
    public float shoot_cooldown_left = 0;
    public int max_hp = 1;
    public int hp;
    public int damage = 1;
    public int collision_damage = 1;
    public int projectile_speed = 1000;
    public float projectile_scale = 0.1f;
    public float projectile_duration = 1;
    /* animation */
    float death_anim_total = 0.5f;
    float death_anim_secs = 0;
    float damage_anim_remaining_secs = 0;
    bool dead = false;

    void Awake()
    {
        game_controller = transform.parent.gameObject;
        path_to_player = new NavMeshPath();
        rb = GetComponent<Rigidbody>();
        nma = GetComponent<NavMeshAgent>();
        nma.enabled = false;
        material = GetComponent<Renderer>().material;
        color_orig = material.color;
        collider = GetComponent<Collider>();
    }

    void Start()
    {
        hp = max_hp;
        bar_hp_max = Instantiate(bar_prefab, transform.position + Vector3.up, Quaternion.identity, game_controller.transform);
        bar_hp_cur = Instantiate(bar_prefab, transform.position + Vector3.up, Quaternion.identity, game_controller.transform);
        bar_hp_max.GetComponent<Renderer>().material.color = Color.red;
        bar_hp_cur.GetComponent<Renderer>().material.color = Color.green;
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

        if (dead)
            return;

        Vector3 diff_vec = player_transf.position - transform.position;

        Transform hp_max_t = bar_hp_max.GetComponent<Transform>();
        Transform hp_cur_t = bar_hp_cur.GetComponent<Transform>();

        float hp_cur_perc = (float)hp / max_hp;
        hp_cur_t.localScale = new Vector3(hp_cur_perc, hp_cur_t.localScale.y, hp_cur_t.localScale.z);
        // hp_max_t.localScale = new Vector3(1 - hp_cur_perc, hp_max_t.localScale.y, hp_max_t.localScale.z);

        float hp_cur_x = (1 - hp_cur_perc) / 2;
        // float hp_max_x = hp_cur_perc;

        Vector3 perp = Vector3.Cross(-diff_vec, transform.up);

        hp_max_t.SetPositionAndRotation(transform.position + Vector3.up * 1.3f,
                player_transf.rotation);
        hp_cur_t.SetPositionAndRotation(transform.position + Vector3.up * 1.3f + player_transf.forward * -0.001f - player_transf.right * hp_cur_x,
                player_transf.rotation);

        if (landed && player.activeInHierarchy) {
            float euclidean_dist = (float)Math.Sqrt(Vector3.Dot(diff_vec, diff_vec));
            if (euclidean_dist > aggro_range)
                return;

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
        if (dead)
            return;
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
        dead = true;
        collider.enabled = false;
        float r = (rng.Next() % 100 + 1) / 100.0f;
        if (r <= drop_chance) {
            Instantiate(reward_prefab, new Vector3(transform.position.x, 1, transform.position.z), Quaternion.identity, game_controller.transform);
        }
        death_anim_secs = death_anim_total;
        Destroy(bar_hp_cur);
        Destroy(bar_hp_max);
        Destroy(gameObject, death_anim_total);
    }
}
