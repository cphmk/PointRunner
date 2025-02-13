using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class MazeGenerator : MonoBehaviour
{
    public GameObject floor_prefab;
    public GameObject wall_prefab;
    public GameObject player_prefab;
    public GameObject enemy_prefab;
    public GameObject minimap_marker_prefab;
    public int wall_w, wall_h;
    public int minimap_cam_distance;
    public int enemies_max;
    public float enemies_chance;

    GameObject game_controller;

    Maze maze;
    List<GameObject> walls = new List<GameObject>();
    List<GameObject> enemies = new List<GameObject>();
    GameObject main_cam;
    GameObject floor;
    Transform floor_transf;
    GameObject player;
    GameObject minimap_cam;
    GameObject minimap_marker;
    System.Random rng = new System.Random();

    Mesh mesh;
    MeshFilter mesh_filter;
    MeshCollider mesh_collider;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    NavMeshSurface nav_mesh_surface;

    void Awake()
    {
        game_controller = GameObject.Find("GameController");

        main_cam = GameObject.Find("Main Camera");
        Destroy(main_cam);

        floor = GameObject.Find("Floor");
        floor_transf = floor.GetComponent<Transform>();
        minimap_cam = GameObject.Find("MiniMapCamera");
        minimap_marker = Instantiate(minimap_marker_prefab, new Vector3(0, 0, 0), Quaternion.identity, transform);

        mesh_filter = GetComponent<MeshFilter>();
        mesh = GetComponent<MeshFilter>().mesh;
        mesh_collider = GetComponent<MeshCollider>();

        nav_mesh_surface = GetComponent<NavMeshSurface>();
    }

    void Start()
    {
    }

    void Update()
    {
        Vector3 player_pos = player.GetComponent<Transform>().position;
        minimap_cam.GetComponent<Transform>().position = new Vector3(player_pos.x, player_pos.y + minimap_cam_distance, player_pos.z);
        minimap_marker.GetComponent<Transform>().position = new Vector3(player_pos.x, player_pos.y + minimap_cam_distance - 10, player_pos.z);
    }

    void RegenMaze(Vector2Int dims)
    {
        Destroy(player);
        foreach (var w in walls) {
            Destroy(w);
        }
        walls.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        int num_enemies = 0;
        maze = new Maze(dims.x, dims.y);
        maze.Gen();

        floor_transf.position = new Vector3((maze.width_real / 2.0f) * wall_w, 0.0f, (maze.height_real / 2.0f) * wall_w);
        floor_transf.localScale = new Vector3(maze.width_real, 1.0f, maze.height_real);

        for (int y = 0; y < maze.height_real; ++y) {
            for (int x = 0; x < maze.width_real; ++x) {
                if (y == maze.start.y * 2 + 1 && x == maze.start.x * 2 + 1) {
                    player = Instantiate(player_prefab, new Vector3(x * wall_w + wall_w / 2,
                                floor_transf.position.y + (wall_h / 2.0f) * 10, y * wall_w + wall_w / 2),
                            Quaternion.identity, game_controller.transform);
                }

                else if (maze.walls[x, y]) {
                    float r = ((rng.Next() % 100) + 1) / 100.0f;
                    if (r < enemies_chance && num_enemies++ < enemies_max) {
                        GameObject enemy = Instantiate(enemy_prefab, new Vector3(x * wall_w + wall_w / 2,
                                    floor_transf.position.y + wall_h + 2, y * wall_w + wall_w / 2),
                                Quaternion.identity, game_controller.transform);
                        enemies.Add(enemy);
                    }
                }

                if (maze.walls[x, y])
                    continue;

                AddCubeQuads(x * wall_w, floor_transf.position.y, y * wall_w, wall_w, wall_h, x, y);
            }
        }

        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        Mesh mesh_for_collider = new Mesh();
        mesh_for_collider.vertices = vertices.ToArray();
        mesh_for_collider.triangles = triangles.ToArray();
        mesh_for_collider.RecalculateBounds();
        mesh_for_collider.RecalculateNormals();
        mesh_collider.sharedMesh = mesh_for_collider;

        nav_mesh_surface.BuildNavMesh();

        Debug.Log(mesh.vertices.Length);
        Debug.Log(mesh.triangles.Length);
        Debug.Log("number of enemies: " + enemies.Count);
    }

    void AddCubeQuads(float x, float y, float z, float w, float h, int maze_x, int maze_y)
    {
        int[] indices = { 0, 1, 2, 0, 2, 3 };
        Vector2[] uv = { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0) };

        Vector3[] top = {
            new Vector3(x, y + h, z),
            new Vector3(x, y + h, z + w), // once
            new Vector3(x + w, y + h, z + w),
            new Vector3(x + w, y + h, z), // once
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

        if (maze_y >= maze.height - 1 || maze.walls[maze_x, maze_y + 1]) {
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
        if (maze_x >= maze.width - 1 || maze.walls[maze_x + 1, maze_y]) {
            triangles.InsertRange(0, indices.Select(i => i + vertices.Count));
            vertices.InsertRange(0, right);
            uvs.InsertRange(0, uv);
        }
    }
}

enum Direction { North, East, South, West }

class Maze
{
    public int width, height;
    public int width_real, height_real;
    public Vector2Int start, end;
    public bool[,] walls;

    public static readonly Vector2Int[] dirs = {new Vector2Int( 0, 1), new Vector2Int( 1, 0), new Vector2Int( 0, -1), new Vector2Int( -1, 0)};

    public Maze(int w, int h) { width = w; height = h; width_real = w * 2 + 1; height_real = h * 2 + 1; }

    public void Gen()
    {
        System.Random rng = new System.Random();
        start = new Vector2Int(rng.Next() % width, rng.Next() % height);

        Debug.Log("start: " + start.ToString() + " in cell: " + (new Vector2Int(start.x * 2 + 1, start.y * 2 + 1)).ToString());

        bool[,] vis = new bool[width, height];
        walls = new bool[width_real, height_real];
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
