using UnityEngine;
using UnityEngine.Audio;

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

    void Update()
    {
        // If Time.timeScale is 0, the game is paused. Don't shoot!
        if (Time.timeScale == 0) return;
        bool shootInput;

        if (isAutomatic)
        {
            shootInput = Input.GetButton("Fire1");
        }
        else
        {
            shootInput = Input.GetMouseButtonDown(0);
        }

        if (shootInput && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        Vector3 shootDirection = firePoint.right;

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;

        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);

        if (firePoint.lossyScale.x < 0)
        {
            bulletRotation = Quaternion.Euler(0, 0, angle+180);
        }

        Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        if (isAutomatic)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
        }
        audioSource.PlayOneShot(shootSound);
    }
}