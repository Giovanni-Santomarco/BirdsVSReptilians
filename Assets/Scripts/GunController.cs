using UnityEngine;
using UnityEngine.Audio;

//this class stores some properties of the gun and basing on them, allowa the gun owner to shoot
public class GunController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    public AudioClip shootSound;
    private AudioSource audioSource;

    public bool isAutomatic = false;

    private float nextShotTime;

    void Start()
    {
        // Get the AudioSource component from the weapon
        audioSource = GetComponent<AudioSource>();
    }

    //shoots only if the fire rate allows to
    public void Shoot()
    {
        if (!(Time.time >= nextShotTime))
            return;
        Vector3 shootDirection = firePoint.right;

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;

        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);

        if (firePoint.lossyScale.x < 0)
        {
            bulletRotation = Quaternion.Euler(0, 0, angle+180);
        }

        Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        nextShotTime = Time.time + fireRate;
        if (isAutomatic)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
        }
        audioSource.PlayOneShot(shootSound);
    }
}