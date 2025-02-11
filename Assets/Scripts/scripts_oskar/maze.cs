using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject floor;
    public GameObject wall_prefab;
    public GameObject player_prefab;
    public int maze_w, maze_h;
    public int wall_w, wall_h;

    Maze maze;
    List<GameObject> walls = new List<GameObject>();
    GameObject player;

    void Start()
    {
        RegenMaze();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) {
            RegenMaze();
        }
    }

    void RegenMaze()
    {
        Destroy(player);
        foreach (var w in walls) {
            Destroy(w);
        }
        walls.Clear();

        maze = new Maze(maze_w, maze_h);
        maze.Gen();

        for (int y = 0; y < maze.height_real; ++y) {
            for (int x = 0; x < maze.width_real; ++x) {
                if (y == maze.start.y * 2 + 1 && x == maze.start.x * 2 + 1) {
                    player = Instantiate(player_prefab, new Vector3(x * wall_w, floor.GetComponent<Transform>().position.y + wall_h, y * wall_w),
                        Quaternion.identity, transform);
                    player.GetComponent<Transform>().localScale = new Vector3(wall_w, wall_h, wall_w);

                }
                if (maze.walls[x, y])
                    continue;
                GameObject new_wall = Instantiate(wall_prefab,
                        new Vector3(x * wall_w, floor.GetComponent<Transform>().position.y + wall_h, y * wall_w),
                        Quaternion.identity, transform);
                new_wall.transform.localScale = new Vector3(wall_w, wall_h, wall_w);
                walls.Add(new_wall);
            }
        }
    }
}

enum Direction { North, East, South, West }

class Cell
{
    public Vector2Int position = new Vector2Int(0, 0);
    public bool[] paths = new bool[4];
}

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

        Cell[,] cells = new Cell[width, height];
        bool[,] vis = new bool[width, height];
        walls = new bool[width_real, height_real];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // string str0 = "";
        // for (int i = 0; i < height; i++) {
        //     for (int j = 0; j < width; j++) {
        //         str0 += vis[j, i].ToString();
        //         str0 += ',';
        //     }
        //     str0 += '\n';
        // }
        // Debug.Log(str0);

        int n_vis = 1;
        stack.Push(start);
        while (n_vis < width * height) {
            Vector2Int p = stack.Peek();
            // Debug.Log("visiting " + p.ToString());
            vis[p.x, p.y] = true;
            Vector2Int p_w = new Vector2Int(p.x * 2 + 1, p.y * 2 + 1);
            walls[p_w.x, p_w.y] = true;

            // Debug.Log(p.ToString());
            int i, r = rng.Next() % 4;
            for (i = 0; i < 4; ++i) {
                Vector2Int p_n = p + dirs[(i + r) % 4];
                Vector2Int p_n_w = new Vector2Int(p.x * 2 + 1, p.y * 2 + 1) + dirs[(i + r) % 4];
                if (!is_in(p_n) || vis[p_n.x, p_n.y])
                    continue;

                // Debug.Log("pushing " + p_n.ToString());
                vis[p_n.x, p_n.y] = true;
                walls[p_n_w.x, p_n_w.y] = true;
                // Debug.Log(p_n.ToString());
                // walls[p_n.x, p_n.y] = true;
                stack.Push(p_n);
                ++n_vis;
                break;
            }
            if (i == 4)
                stack.Pop();
            // Debug.Log(n_vis);
        }

        string str = "";
        for (int y = height_real - 1; y >= 0; --y) {
            for (int x = 0; x < width_real; ++x) {
                str += (walls[x,y]) ? 'o' : 'w';
                if (x < width * 2)
                    str += ',';
            }
            str += '\n';
        }
        Debug.Log(str);
    }

    private bool is_in(Vector2Int p)
    {
        return 0 <= p.x && p.x < width && 0 <= p.y && p.y < height;
    }
}
