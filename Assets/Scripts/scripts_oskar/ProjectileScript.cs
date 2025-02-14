using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public int damage = 1;
    public int speed = 1000;
    public float duration = 1;
    public float scale = 0.1f;
    public Color color = Color.red;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Destroy(gameObject, duration);
        transform.localScale = new Vector3(scale, scale, scale);
        rb.AddForce(transform.forward * speed);
        GetComponent<Renderer>().material.color = color;
    }

    void OnCollisionEnter(Collision collision_data)
    {
        foreach (ContactPoint c_point in collision_data) {
            if (!c_point.otherCollider.gameObject.CompareTag("Player"))
                continue;
            c_point.otherCollider.gameObject.SendMessage("OnProjectileHit", damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}
