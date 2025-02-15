using System;
using UnityEngine;

public class RewardScript : MonoBehaviour
{
    public Reward reward = new Reward();
    Material material;

    void Start()
    {
        material = GetComponent<Renderer>().material;
        Destroy(gameObject, 10);
    }

    void Update()
    {
        material.color = new Color(material.color.r, material.color.g, material.color.b, Math.Abs((float)Math.Sin(Time.time * 2)));
        transform.localEulerAngles = new Vector3(90, Math.Abs((float)Math.Sin(Time.time * 2)) * 90.0f, 0);
    }

    void OnTriggerEnter(Collider collision_data)
    {
        if (collision_data.gameObject.CompareTag("Player")) {
            collision_data.gameObject.SendMessage("OnReward", reward, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}
