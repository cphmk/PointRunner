using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject floor;
    public GameObject wall_prefab;
    public GameObject player_prefab;
    public GameObject minimap_marker_prefab;
    public int maze_w, maze_h;
    public int wall_w, wall_h;
    public int minimap_cam_distance;

    Maze maze;
    List<GameObject> walls = new List<GameObject>();
    Transform floor_transf;
    GameObject player;
    GameObject minimap_cam;
    GameObject minimap_marker;

    Mesh mesh;
    MeshFilter mesh_filter;
    MeshCollider mesh_collider;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    void Start()
    {
        if (maze_w > 80)
            maze_w = 80;
        if (maze_h > 80)
            maze_h = 80;

        floor_transf = floor.GetComponent<Transform>();
        minimap_cam = GameObject.Find("MiniMapCamera");
        minimap_marker = Instantiate(minimap_marker_prefab, new Vector3(0, 0, 0), Quaternion.identity, transform);

        mesh_filter = GetComponent<MeshFilter>();
        mesh = GetComponent<MeshFilter>().mesh;
        mesh_collider = GetComponent<MeshCollider>();

        RegenMaze();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            RegenMaze();
        }

        Vector3 player_pos = player.GetComponent<Transform>().position;
        minimap_cam.GetComponent<Transform>().position = new Vector3(player_pos.x, player_pos.y + minimap_cam_distance, player_pos.z);
        minimap_marker.GetComponent<Transform>().position = new Vector3(player_pos.x, player_pos.y + minimap_cam_distance - 10, player_pos.z);
    }

    void RegenMaze()
    {
        Destroy(player);
        foreach (var w in walls) {
            Destroy(w);
        }
        walls.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        maze = new Maze(maze_w, maze_h);
        maze.Gen();

        for (int y = 0; y < maze.height_real; ++y) {
            for (int x = 0; x < maze.width_real; ++x) {
                if (y == maze.start.y * 2 + 1 && x == maze.start.x * 2 + 1) {
                    player = Instantiate(player_prefab, new Vector3(x * wall_w + wall_w / 2,
                                floor_transf.position.y + (wall_h / 2.0f) * 20, y * wall_w + wall_w / 2),
                        Quaternion.identity, transform);
                }

                if (maze.walls[x, y])
                    continue;

                AddCubeQuads(x * wall_w, floor_transf.position.y + floor_transf.localScale.y / 2.0f , y * wall_w, wall_w, wall_h, x, y);

                // GameObject new_wall = Instantiate(wall_prefab,
                //         new Vector3(x * wall_w, floor.GetComponent<Transform>().position.y + wall_h / 2.0f, y * wall_w),
                //         Quaternion.identity, transform);
                // new_wall.transform.localScale = new Vector3(wall_w, wall_h, wall_w);

                // foreach (var t in new_wall.GetComponent<MeshFilter>().mesh.triangles) {
                //     triangles.Add(t + vertices.Count);
                // }

                // foreach (var v in new_wall.GetComponent<MeshFilter>().mesh.vertices) {
                //     vertices.Add(new Vector3(v.x * wall_w + x * wall_w,
                //                 v.y * wall_h + floor.GetComponent<Transform>().position.y + wall_h / 2.0f,
                //                 v.z * wall_w + y * wall_w));
                // }

                // foreach (var uv in new_wall.GetComponent<MeshFilter>().mesh.uv) {
                //     uvs.Add(uv);
                // }

                // Destroy(new_wall);
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

        if (maze_y >= maze_h - 1 || maze.walls[maze_x, maze_y + 1]) {
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
        if (maze_x >= maze_w - 1 || maze.walls[maze_x + 1, maze_y]) {
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
                Vector2Int p_n_w = new Vector2Int(p.x * 2 + 1, p.y * 2 + 1) + dirs[(i + r) % 4];
                if (!is_in(p_n) || vis[p_n.x, p_n.y])
                    continue;

                vis[p_n.x, p_n.y] = true;
                walls[p_n_w.x, p_n_w.y] = true;
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
