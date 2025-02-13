using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    GameObject maze;

    public Vector2Int maze_size;

    /* game variables */
    int player_hp = 100;
    int player_hp_max = 100;
    int player_dmg = 1;
    int player_speed = 1;
    /* UI */
    GameObject UI;
    GameObject UI_overlay;
    Image UI_overlay_image;
    TextMeshProUGUI text_player_stats;

    float damage_blink_total_secs;
    float damage_blink_remaining_secs;

    void Start()
    {
        maze = GameObject.Find("Maze");

        UI = GameObject.Find("UI");
        UI_overlay = GameObject.Find("Overlay");
        UI_overlay_image = UI_overlay.GetComponent<Image>();

        text_player_stats = GameObject.Find("TextStats").GetComponent<TextMeshProUGUI>();

        Restart();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            Restart();
        }

        text_player_stats.text = string.Format("HP: {0}/{1}", player_hp, player_hp_max);

        if (damage_blink_remaining_secs > 0) {
            damage_blink_remaining_secs -= Time.deltaTime;
            UI_overlay_image.color = new Color(1, 0, 0, 0.25f * Math.Abs((float)Math.Sin(Time.time * 20)));
        }
        else {
            UI_overlay_image.color = new Color(1, 0, 0, 0);
        }
    }

    void EnemyPlayerCollision(Transform enemy_transf)
    {
        // Debug.Log("enemy collided with player at " + enemy_transf.position.ToString());
        DamagePlayer(1);
    }

    void DamagePlayerEvent(int d)
    {
        DamagePlayer(d);
    }

    void Restart()
    {
        player_hp = player_hp_max;
        maze.SendMessage("RegenMaze", maze_size);
    }

    void GoNext()
    {

    }

    void DamagePlayer(int d)
    {
        player_hp -= d;
        if (player_hp < 1) {
            Debug.Log("DEAD");
        }
        damage_blink_remaining_secs = 0.25f;
    }
}
