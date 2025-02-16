using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;
using TMPro;

public class GameController : MonoBehaviour
{
    /* prefabs */
    public GameObject player_prefab;
    public GameObject enemy_capsule_prefab;
    public GameObject floor_prefab;
    public GameObject minimap_marker_prefab;
    public GameObject goal_prefab;
    public GameObject spawn_prefab;
    public GameObject exclamation_mark_prefab;
    /* runtime game objects */
    GameObject go_main_cam;
    GameObject go_maze;
    GameObject go_floor;
    GameObject go_player;
    GameObject go_minimap_cam;
    GameObject go_minimap_marker;
    GameObject go_minimap_marker_spawn;
    GameObject go_minimap_marker_goal;
    GameObject go_goal;
    GameObject go_goal_excl;
    GameObject go_spawn;
    GameObject go_spawn_excl;
    List<GameObject> go_enemies;

    Camera main_cam;
    Camera player_cam;

    Transform floor_transform;

    public int maze_base_width;
    public int maze_base_height;
    int maze_width;
    int maze_height;
    public int maze_wall_width;
    public int maze_wall_height;
    public int minimap_cam_distance = 50;

    /* game variables */
    Maze maze;
    MazeMeshGenerator maze_mesh_generator;

    System.Random game_rng = new System.Random();

    bool is_playing = false;
    bool is_paused = false;
    int game_level = 1;
    int game_points = 0;
    int game_num_enemies = 10;
    PlayerScript game_player_props;
    /* UI */
    float UI_refresh_time = 4;
    float UI_refresh_time_cooldown = 0;
    GameObject UI;
    GameObject UI_overlay;
    Image UI_overlay_image;
    TextMeshProUGUI UI_game_stats_text;
    TextMeshProUGUI UI_player_stats_text;
    TextMeshProUGUI goal_text;
    GameObject go_UI_player_stats;
    GameObject go_UI_game_stats;
    GameObject go_UI_minimap;
    GameObject go_UI_play_button;
    GameObject go_UI_resume_button;
    GameObject go_UI_mainmenu_button;
    GameObject go_UI_title;
    Button UI_play_button;
    Button UI_resume_button;
    Button UI_mainmenu_button;
    TextMeshProUGUI UI_title;
    Color goal_text_color = Color.green;
    float goal_text_fade_time = 5;
    float goal_text_fade = 0;

    float damage_blink_total_secs;
    float damage_blink_remaining_secs;
    float reward_blink_total_secs;
    float reward_blink_remaining_secs;

    void Start()
    {
        go_main_cam = GameObject.Find("Main Camera");
        main_cam = go_main_cam.GetComponent<Camera>();
        go_maze = GameObject.Find("Maze");
        go_floor = GameObject.Find("Floor");
        go_minimap_cam = GameObject.Find("MiniMapCamera");
        go_minimap_marker = Instantiate(minimap_marker_prefab, new Vector3(0, 0, 0), Quaternion.identity, transform);
        go_enemies = new List<GameObject>();

        floor_transform = go_floor.GetComponent<Transform>();

        UI = GameObject.Find("UI");
        UI_overlay = GameObject.Find("Overlay");
        UI_overlay_image = UI_overlay.GetComponent<Image>();
        go_UI_play_button = GameObject.Find("PlayButton");
        UI_play_button = go_UI_play_button.GetComponent<Button>();
        go_UI_resume_button = GameObject.Find("ResumeButton");
        UI_resume_button = go_UI_resume_button.GetComponent<Button>();
        go_UI_mainmenu_button = GameObject.Find("MainMenuButton");
        UI_mainmenu_button = go_UI_mainmenu_button.GetComponent<Button>();
        go_UI_minimap = GameObject.Find("MiniMap");
        go_UI_title = GameObject.Find("TitleText");
        UI_title = go_UI_title.GetComponent<TextMeshProUGUI>();

        go_UI_player_stats = GameObject.Find("PlayerStatsText");
        go_UI_game_stats = GameObject.Find("GameStatsText");
        UI_player_stats_text = go_UI_player_stats.GetComponent<TextMeshProUGUI>();
        UI_game_stats_text = go_UI_game_stats.GetComponent<TextMeshProUGUI>();
        goal_text = GameObject.Find("GoalText").GetComponent<TextMeshProUGUI>();
        goal_text.text = "";

        UI_play_button.onClick.AddListener(() => { StopGame(); NewMaze(); StartGame();});
        UI_resume_button.onClick.AddListener(() => { ResumeGame();});
        UI_mainmenu_button.onClick.AddListener(() => { SceneManager.LoadScene("Menu");});

        go_UI_resume_button.SetActive(false);

        NewMaze();
        StopGame();
    }

    void Update()
    {
        if (is_playing) {
            Vector3 player_pos = go_player.GetComponent<Transform>().position;

            if (game_player_props != null) {
                UI_player_stats_text.text = GetPlayerStatsText();
                UI_game_stats_text.text = GetGameStatsText();
            }
            else
                UI_player_stats_text.text = "";

            if (damage_blink_remaining_secs > 0) {
                damage_blink_remaining_secs -= Time.deltaTime;
                UI_overlay_image.color = new Color(1, 0, 0, 0.25f * Math.Abs((float)Math.Sin(Time.time * 20)));
            }

            else if (reward_blink_remaining_secs > 0) {
                reward_blink_remaining_secs -= Time.deltaTime;
                UI_overlay_image.color = new Color(0, 1, 0, 0.25f * Math.Abs((float)Math.Sin(Time.time * 20)));
            }

            else {
                UI_overlay_image.color = new Color(1, 0, 0, 0);
            }

            if (game_player_props.hp < 1) {
                Debug.Log("DEAD");
                StopGame();
            }
            go_minimap_marker.GetComponent<Transform>().position = new Vector3(player_pos.x, player_pos.y + minimap_cam_distance - 10, player_pos.z);
        }

        else if (!is_paused) {
            UI_refresh_time_cooldown -= Time.deltaTime;
            if (UI_refresh_time_cooldown <= 0) {
                UI_refresh_time_cooldown = UI_refresh_time;
                NewMaze();
            }
        }

        UpdateMenuCam();

        if (Input.GetKeyDown(KeyCode.R)) {
            OnGoal();
            RewardPlayer();
        }

        if (Input.GetKeyDown(KeyCode.U)) {
            RewardPlayer();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            PauseGame();
        }


        if (goal_text_fade > 0) {
            goal_text_fade -= Time.deltaTime;
            goal_text.color = goal_text_color - new Color(0, 0, 0, (goal_text_fade_time - goal_text_fade) / goal_text_fade_time);
        }
        else
            goal_text.color = goal_text_color - new Color(0, 0, 0, 1);

    }

    void NewMaze()
    {
        Destroy(go_goal);
        Destroy(go_goal_excl);
        Destroy(go_minimap_marker_goal);
        Destroy(go_spawn);
        Destroy(go_spawn_excl);
        Destroy(go_minimap_marker_spawn);

        maze_width = maze_base_width + (int)(Math.Sqrt(game_level)) + (game_rng.Next() % 3);
        maze_height = maze_base_height + (int)(Math.Sqrt(game_level)) + (game_rng.Next() % 3);
        maze = new Maze(maze_width, maze_height);
        maze_mesh_generator = new MazeMeshGenerator();
        maze.Gen();

        floor_transform.position = new Vector3((maze.width_world / 2.0f) * maze_wall_width, 0.0f, (maze.height_world / 2.0f) * maze_wall_width);
        floor_transform.localScale = new Vector3(maze.width_world, 1.0f, maze.height_world);

        maze_mesh_generator.GenMesh(maze, floor_transform.position.y, maze_wall_width, maze_wall_height);

        MeshFilter mesh_filter = go_maze.GetComponent<MeshFilter>();
        Mesh mesh = go_maze.GetComponent<MeshFilter>().mesh;
        MeshCollider mesh_collider = go_maze.GetComponent<MeshCollider>();
        NavMeshSurface nav_mesh_surface = go_maze.GetComponent<NavMeshSurface>();

        mesh.Clear();

        mesh.vertices = maze_mesh_generator.vertices.ToArray();
        mesh.triangles = maze_mesh_generator.triangles.ToArray();
        mesh.uv = maze_mesh_generator.uvs.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        Mesh mesh_for_collider = new Mesh();
        mesh_for_collider.vertices = maze_mesh_generator.vertices.ToArray();
        mesh_for_collider.triangles = maze_mesh_generator.triangles.ToArray();
        mesh_for_collider.RecalculateBounds();
        mesh_for_collider.RecalculateNormals();
        mesh_collider.sharedMesh = mesh_for_collider;

        Vector3 goal_pos = new Vector3(maze.finish_world.x * maze_wall_width + maze_wall_width / 2,
                    floor_transform.position.y, maze.finish_world.y * maze_wall_width + maze_wall_width / 2);
        Vector3 spawn_pos = new Vector3(maze.start_world.x * maze_wall_width + maze_wall_width / 2,
                    floor_transform.position.y, maze.start_world.y * maze_wall_width + maze_wall_width / 2);
        Vector3 player_pos = spawn_pos + new Vector3(-0.5f, 2, 0);

        go_goal = Instantiate(goal_prefab, goal_pos, Quaternion.identity, transform);
        go_goal_excl = Instantiate(exclamation_mark_prefab, goal_pos + Vector3.up * maze_wall_height * 1.5f + new Vector3(-12, 0, -12), Quaternion.identity, transform);
        foreach (Transform t in go_goal_excl.transform) {
            t.gameObject.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.5f);
        }
        go_minimap_marker_goal = Instantiate(minimap_marker_prefab, goal_pos + new Vector3(0, minimap_cam_distance - 10, 0), Quaternion.identity);
        go_minimap_marker_goal.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1);

        go_spawn = Instantiate(spawn_prefab, spawn_pos, Quaternion.identity, transform);
        go_spawn_excl = Instantiate(exclamation_mark_prefab, spawn_pos + Vector3.up * maze_wall_height * 1.5f + new Vector3(-12, 0, -12), Quaternion.identity, transform);
        foreach (Transform t in go_spawn_excl.transform) {
            t.gameObject.GetComponent<Renderer>().material.color = new Color(0, 0, 1, 0.5f);
        }
        go_minimap_marker_spawn = Instantiate(minimap_marker_prefab, spawn_pos + new Vector3(0, minimap_cam_distance - 10, 0), Quaternion.identity);
        go_minimap_marker_spawn.GetComponent<Renderer>().material.color = new Color(0, 0, 1, 1);

        nav_mesh_surface.BuildNavMesh();

        go_minimap_cam.GetComponent<Transform>().position = floor_transform.position + new Vector3(0, minimap_cam_distance, 0);
        go_minimap_cam.GetComponent<Camera>().orthographicSize = Math.Max(maze_wall_width * maze.width_world / 2, maze_wall_width * maze.height_world / 2);

        go_minimap_marker.SetActive(false);
        go_minimap_marker_spawn.SetActive(false);
        go_minimap_marker_goal.SetActive(false);

//         Debug.Log("maze mesh vertices: " + mesh.vertices.Length);
//         Debug.Log("maze mesh triangles: " + mesh.triangles.Length / 3);
    }

    void StartGame()
    {
        go_main_cam.SetActive(false);
        go_UI_play_button.SetActive(false);
        go_UI_mainmenu_button.SetActive(false);
        go_UI_title.SetActive(false);
        go_UI_minimap.SetActive(true);
        go_UI_player_stats.SetActive(true);
        go_UI_game_stats.SetActive(true);
        go_minimap_marker.SetActive(true);
        go_minimap_marker_spawn.SetActive(true);
        go_minimap_marker_goal.SetActive(true);

        Vector3 goal_pos = new Vector3(maze.finish_world.x * maze_wall_width + maze_wall_width / 2,
                    floor_transform.position.y, maze.finish_world.y * maze_wall_width + maze_wall_width / 2);
        Vector3 spawn_pos = new Vector3(maze.start_world.x * maze_wall_width + maze_wall_width / 2,
                    floor_transform.position.y, maze.start_world.y * maze_wall_width + maze_wall_width / 2);
        Vector3 player_pos = spawn_pos + new Vector3(-0.5f, 2, 0);

        go_player = Instantiate(player_prefab, player_pos, Quaternion.identity, transform);
        game_player_props = go_player.GetComponent<PlayerScript>();
        player_cam = go_player.GetComponent<Camera>();

        game_num_enemies = 2 * (maze.width + game_level) + 2 * (maze.height + game_level);
        for (int i = 0; i < game_num_enemies; i++) {
            int x = game_rng.Next() % maze.width_world;
            int y = game_rng.Next() % maze.height_world;

            while (!maze.walls[x, y] || (x == maze.start_world.x && y == maze.start_world.y)
                    || (x == maze.finish_world.x && y == maze.finish_world.y)) {
                x = game_rng.Next() % maze.width_world;
                y = game_rng.Next() % maze.height_world;
            }

            float offs = 0.1f * ((game_rng.Next() % 6)) + 0.25f;

            GameObject enemy = Instantiate(enemy_capsule_prefab, new Vector3(x * maze_wall_width + maze_wall_width * offs,
                        floor_transform.position.y + floor_transform.localScale.y + maze_height * offs + 2, y * maze_wall_width + maze_wall_width * offs),
                    Quaternion.identity, transform);
            EnemyCapsuleScript enemy_props = enemy.GetComponent<EnemyCapsuleScript>();
            enemy_props.max_hp = 1 + 3 * game_level;
            enemy_props.damage = 1 + 2 * game_level;
            enemy_props.aggro_range = 15 + 2 * game_level;
            enemy_props.shoot_cooldown = Math.Max(0.5f - (0.02f * game_level), 0.1f);
            enemy_props.collision_damage = 1 + game_level;
            enemy_props.projectile_speed = 1000 + 50 * game_level;
            go_enemies.Add(enemy);
        }

        is_playing = true;
    }

    void PauseGame()
    {
        is_playing = false;
        is_paused = true;
        go_player.SetActive(false);
        go_main_cam.SetActive(true);
        go_UI_minimap.SetActive(false);
        go_UI_play_button.SetActive(true);
        go_UI_mainmenu_button.SetActive(true);
        go_UI_title.SetActive(true);

        go_minimap_marker.SetActive(false);
        go_minimap_marker_spawn.SetActive(false);
        go_minimap_marker_goal.SetActive(false);
        go_minimap_marker.SetActive(false);
        go_UI_resume_button.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }

    void ResumeGame()
    {
        is_playing = true;
        is_paused = false;
        go_player.SetActive(true);
        go_main_cam.SetActive(false);
        go_UI_minimap.SetActive(true);
        go_UI_play_button.SetActive(false);
        go_UI_title.SetActive(false);
        go_minimap_marker.SetActive(true);
        go_minimap_marker_spawn.SetActive(true);
        go_minimap_marker_goal.SetActive(true);
        go_minimap_marker.SetActive(true);
        go_UI_resume_button.SetActive(false);
        go_UI_mainmenu_button.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    void StopGame()
    {
        game_level = 1;
        game_points = 0;

        CleanUp();

        is_playing = false;
        go_main_cam.SetActive(true);
        go_UI_minimap.SetActive(false);
        go_UI_play_button.SetActive(true);
        go_UI_resume_button.SetActive(false);
        go_UI_mainmenu_button.SetActive(true);
        go_UI_title.SetActive(true);
        go_UI_player_stats.SetActive(false);
        go_UI_game_stats.SetActive(false);

        go_minimap_marker.SetActive(false);
        go_minimap_marker_spawn.SetActive(false);
        go_minimap_marker_goal.SetActive(false);

        go_minimap_marker.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
    }

    void CleanUp()
    {
        Destroy(go_spawn);
        Destroy(go_goal);
        Destroy(go_player);
        foreach (var e in go_enemies)
            Destroy(e);
    }

    void DamagePlayer(int d)
    {
        damage_blink_remaining_secs = 0.25f * (float)Math.Sqrt(d);
    }

    void RewardPlayer()
    {
        game_points++;
        reward_blink_remaining_secs = 0.25f;
    }

    string GetPlayerStatsText()
    {
        return string.Format("HP: {0}/{1} DMG: {2} RELOAD SPEED: {3:F1} PROJ SPEED/DUR/SCALE: {4:F1}/{5:F1}/{6:F1}",
                game_player_props.hp, game_player_props.hp_max,
                game_player_props.damage, game_player_props.shoot_cooldown,
                game_player_props.projectile_speed, game_player_props.projectile_duration, game_player_props.projectile_scale);
    }

    void OnGoal()
    {
        game_level++;

        PlayerStats ps = new PlayerStats();
        ps.hp = game_player_props.hp;
        ps.hp_max = game_player_props.hp_max;
        ps.damage = game_player_props.damage;
        ps.projectile_speed = game_player_props.projectile_speed;
        ps.projectile_scale = game_player_props.projectile_scale;
        ps.projectile_duration = game_player_props.projectile_duration;
        ps.shoot_cooldown = game_player_props.shoot_cooldown;

        CleanUp();
        NewMaze();
        StartGame();

        game_player_props.hp = ps.hp_max;
        game_player_props.hp_max = ps.hp_max;
        game_player_props.damage = ps.damage;
        game_player_props.projectile_speed = ps.projectile_speed;
        game_player_props.projectile_scale = ps.projectile_scale;
        game_player_props.projectile_duration = ps.projectile_duration;
        game_player_props.shoot_cooldown = ps.shoot_cooldown;

        RewardPlayer();

        goal_text.text = "LEVEL " + game_level.ToString();
        goal_text.color = goal_text_color;
        goal_text_fade = goal_text_fade_time;
    }

    string GetGameStatsText()
    {
        return string.Format("LEVEL: {0} SCORE: {1}", game_level, game_points);
    }

    void UpdateMenuCam()
    {
        float camera_dist = 100;
        float camera_height = 150;
        float angle = Time.time * 1;
        float x = (float)Math.Cos(angle) * camera_dist + floor_transform.position.x;
        float z = (float)Math.Sin(angle) * camera_dist + floor_transform.position.z;
        Vector3 camera_pos = new Vector3(x, camera_height, z);
        main_cam.transform.SetPositionAndRotation(camera_pos,
                Quaternion.LookRotation(floor_transform.position - camera_pos, Vector3.up));
    }
}

class PlayerStats
{
    public float shoot_cooldown = 0.5f;
    public int hp = 100;
    public int hp_max = 100;
    public int damage = 1;
    public int collision_damage = 1;
    public int projectile_speed = 2000;
    public float projectile_scale = 0.1f;
    public float projectile_duration = 1;
}

class MazeMeshGenerator
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();

    int[] indices = { 0, 1, 2, 0, 2, 3 };
    Vector2[] uv = { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0) };

    public void GenMesh(Maze maze, float floor_y, int wall_w, int wall_h)
    {
        for (int y = 0; y < maze.height_world; ++y)
            for (int x = 0; x < maze.width_world; ++x)
                if (!maze.walls[x, y])
                    AddCubeQuads(x * wall_w, floor_y, y * wall_w, wall_w, wall_h, x, y, maze);
    }

    void AddCubeQuads(float x, float y, float z, float w, float h, int maze_x, int maze_y, Maze maze)
    {
        Vector3[] top = {
            new Vector3(x, y + h, z),
            new Vector3(x, y + h, z + w),
            new Vector3(x + w, y + h, z + w),
            new Vector3(x + w, y + h, z),
        };

        Vector3[] left = {
            new Vector3(x, y, z + w),
            new Vector3(x, y + h, z + w),
            new Vector3(x, y + h, z),
            new Vector3(x, y, z),
        };

        Vector3[] right = {
            new Vector3(x + w, y, z),
            new Vector3(x + w, y + h, z),
            new Vector3(x + w, y + h, z + w),
            new Vector3(x + w, y, z + w),
        };

        Vector3[] front = {
            new Vector3(x, y, z),
            new Vector3(x, y + h, z),
            new Vector3(x + w, y + h, z),
            new Vector3(x + w, y, z),
        };

        Vector3[] back = {
            new Vector3(x + w, y, z + w),
            new Vector3(x + w, y + h, z + w),
            new Vector3(x, y + h, z + w),
            new Vector3(x, y, z + w),
        };

        triangles.InsertRange(0, indices.Select(i => i + vertices.Count));
        vertices.InsertRange(0, top);
        uvs.InsertRange(0, uv);

        if (maze_y >= maze.height_world - 1 || maze.walls[maze_x, maze_y + 1]) {
            triangles.InsertRange(0, indices.Select(i => i + vertices.Count));
            vertices.InsertRange(0, back);
            uvs.InsertRange(0, uv);
        }
        if (maze_y == 0 || maze.walls[maze_x, maze_y - 1]) {
            triangles.InsertRange(0, indices.Select(i => i + vertices.Count));
            vertices.InsertRange(0, front);
            uvs.InsertRange(0, uv);
        }
        if (maze_x == 0 || maze.walls[maze_x - 1, maze_y]) {
            triangles.InsertRange(0, indices.Select(i => i + vertices.Count));
            vertices.InsertRange(0, left);
            uvs.InsertRange(0, uv);
        }
        if (maze_x >= maze.width_world - 1 || maze.walls[maze_x + 1, maze_y]) {
            triangles.InsertRange(0, indices.Select(i => i + vertices.Count));
            vertices.InsertRange(0, right);
            uvs.InsertRange(0, uv);
        }
    }
}

class Maze
{
    public int width, height;
    public int width_world, height_world;
    public Vector2Int start, finish;
    public Vector2Int start_world, finish_world;
    public bool[,] walls;
    public List<Vector2Int> free_tiles;

    public static readonly Vector2Int[] dirs = {new Vector2Int( 0, 1), new Vector2Int( 1, 0), new Vector2Int( 0, -1), new Vector2Int( -1, 0)};

    public Maze(int w, int h) { width = w; height = h; width_world = w * 2 + 1; height_world = h * 2 + 1; }

    public void Gen()
    {
        System.Random rng = new System.Random();
        start = new Vector2Int(rng.Next() % width, rng.Next() % height);
        start_world = new Vector2Int(start.x * 2 + 1, start.y * 2 + 1);

        // Debug.Log("start: " + start.ToString() + " in cell: " + (new Vector2Int(start.x * 2 + 1, start.y * 2 + 1)).ToString());

        bool[,] vis = new bool[width, height];
        walls = new bool[width_world, height_world];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        int n_vis = 1;
        stack.Push(start);
        while (n_vis < width * height) {
            Vector2Int p = stack.Peek();
            vis[p.x, p.y] = true;
            Vector2Int p_w = new Vector2Int(p.x * 2 + 1, p.y * 2 + 1);
            walls[p_w.x, p_w.y] = true;

            int i, r = rng.Next() % 4;
            for (i = 0; i < 4; ++i) {
                Vector2Int p_n = p + dirs[(i + r) % 4];
                Vector2Int p_n_c = new Vector2Int(p.x * 2 + 1, p.y * 2 + 1) + dirs[(i + r) % 4] * 2;
                Vector2Int p_n_w = new Vector2Int(p.x * 2 + 1, p.y * 2 + 1) + dirs[(i + r) % 4];
                if (!is_in(p_n) || vis[p_n.x, p_n.y])
                    continue;

                vis[p_n.x, p_n.y] = true;
                walls[p_n_w.x, p_n_w.y] = true;
                walls[p_n_c.x, p_n_c.y] = true;
                stack.Push(p_n);
                ++n_vis;
                if (n_vis == width * height) {
                    finish = p_n;
                    finish_world = new Vector2Int(finish.x * 2 + 1, finish.y * 2 + 1);
                }
                break;
            }
            if (i == 4)
                stack.Pop();
        }
    }

    private bool is_in(Vector2Int p)
    {
        return 0 <= p.x && p.x < width && 0 <= p.y && p.y < height;
    }
}
