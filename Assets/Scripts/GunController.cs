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

    private string shooter;


    [Header("Statistiche Proiettile")]
    public int damage = 20;
    public float bulletSpeed = 10f;
    public float bloom = 2f;
    public int numberOfBulletsFiredTogether = 1;



    void Start()
    {
        // Get the AudioSource component from the weapon
        audioSource = GetComponent<AudioSource>();
    }

    //shoots only if the fire rate allows to
    public bool Shoot()
    {
        if (!(Time.time >= nextShotTime))
            return false;
        Vector3 shootDirection = firePoint.right;

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;

        for(int i = 0; i<numberOfBulletsFiredTogether; i++)
        {
            float imprecision = Random.Range(-bloom, bloom);

            Quaternion bulletRotation = Quaternion.Euler(0, 0, angle + imprecision);

            if (firePoint.lossyScale.x < 0)
            {
                bulletRotation = Quaternion.Euler(0, 0, angle + 180 + imprecision);
            }

            // I want to instantiate this by telling if it comes from an enemy or a player
            // ==> bullet does not collide with the same character who shoot it
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
            bullet.GetComponent<Bullet>().Setup(this.shooter, damage, bulletSpeed);
        }

        nextShotTime = Time.time + fireRate;
        if (isAutomatic)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
        }
        audioSource.PlayOneShot(shootSound);

        return true;
    }

    internal void setShooter(string typeOfShooter)
    {
        if (shooter != null) return;
        this.shooter = typeOfShooter;
    }
}