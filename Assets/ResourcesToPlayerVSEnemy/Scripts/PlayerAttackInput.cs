using System;
using UnityEngine;

public class PlayerAttackInput : MonoBehaviour
{
    public event Action StrafeStart;
    public event Action StrafeEnd;
    public event Action Attack;

    public float attackCooldown = 0.5f; // Cooldown-time in seconds
    private float lastAttackTime = 0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StrafeStart?.Invoke();
        }

        if (Input.GetMouseButtonUp(1))
        {
            StrafeEnd?.Invoke();
        }

        // Adding a cooldown-check for sword attacks
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time; // Updating the time of sword attack animation
            Attack?.Invoke();
        }
    }
}

