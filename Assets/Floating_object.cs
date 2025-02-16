using UnityEngine;

public class LootFloating : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatHeight = 0.5f;

    private Vector3 startPos;
    public float rotationSpeed = 75f;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatHeight, 0);
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}