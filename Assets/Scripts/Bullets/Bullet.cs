using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private string shooter; //can be "enemy" or "player", enemy don't shoot enemy

    private int damage;
    private float speed;
    private Rigidbody2D rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // È sempre buona norma prendere i componenti nell'Awake
        rb = GetComponent<Rigidbody2D>();
    }

    // La nostra nuova funzione di Setup universale!
    public void Setup(string typeOfShooter, int bulletDamage, float bulletSpeed)
    {
        shooter = typeOfShooter;
        damage = bulletDamage;
        speed = bulletSpeed;

        // Dato che conosciamo la velocità, possiamo applicarla SUBITO al proiettile
        // (presumendo che il proiettile viaggi in avanti rispetto alla sua rotazione)
        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if the bullet hits an enemy (or the player, TODO), enemy (or player, TODO) loses hp
        bool shooterAndHitAreTheSame = collision.tag == "Player" && this.shooter.Equals("player") || collision.tag == "Enemy" && this.shooter.Equals("enemy");
        bool bulletsHitsBullet = collision.tag == "Bullet";
        bool aCharacterWasHit = collision.tag == "Player" || collision.tag == "Enemy";

        if (!shooterAndHitAreTheSame && aCharacterWasHit)
        {
            LifeCycle enemy = collision.GetComponent<LifeCycle>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        // don't destroy the bullet if it was shoot by the same one who shoot it
        // don't destroy the bulet if it collides with another bullet
        if (!shooterAndHitAreTheSame && !bulletsHitsBullet)
        {
            Destroy(gameObject);
        }
    }
}
