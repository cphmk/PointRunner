using System;
using UnityEngine;

public class Reward
{
    public float shoot_cooldown;
    public int hp;
    public int hp_max;
    public int damage;
    public int projectile_speed;
    public float projectile_scale;
    public float projectile_duration;

    public Reward()
    {
        System.Random rng = new System.Random();
        int n = rng.Next() % 7;
        if (n == 0)
            shoot_cooldown = 0.05f;
        if (n == 1)
            hp = 1;
        if (n == 2)
            hp_max = 1;
        if (n == 3)
            damage = 1;
        if (n == 4)
            projectile_speed = 50;
        if (n == 6)
            projectile_scale = 0.05f;
        if (n == 7)
            projectile_duration = 0.05f;
    }
}
