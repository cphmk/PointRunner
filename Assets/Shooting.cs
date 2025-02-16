using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform muzzle; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Instantiate(prefab, position, rotation)
            ProjectileShooting();

            RaycastShooting();
        }
    }

    void RaycastShooting()
    {
        Vector3 direction = muzzle.forward;
        RaycastHit hit;
        if (Physics.Raycast(muzzle.position, direction, out hit, 1000))
        {
            Debug.Log(hit.transform.gameObject.name);
            
            // spawn hit effect (det man rammer) 
            // Instantiate(bloodEffect, hit.point, bloodEffect.rotation);
        }
    }

    void ProjectileShooting()
    {
        Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);
    }
}