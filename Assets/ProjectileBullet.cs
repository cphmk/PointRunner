using System;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileBullet : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    private void Awake()
    {
        Destroy(gameObject,3);
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(transform.forward * this.speed, ForceMode.Impulse);
    }
}