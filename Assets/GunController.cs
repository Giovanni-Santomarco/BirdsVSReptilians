using UnityEngine;

public class GunController : MonoBehaviour
{
    //public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        
    }
}