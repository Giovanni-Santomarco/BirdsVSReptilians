using System;
using UnityEngine;

public abstract class Bullet : MonoBehaviour
{

    abstract public int getDamageAmount();
    abstract public Rigidbody2D getRigidBody();
    abstract public float getSpeed();

    private string shooter; //can be "enemy" or "player", enemy don't shoot enemy


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        getRigidBody().linearVelocity = getSpeed() * transform.right;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool shooterAndHitAreTheSame = collision.tag == "Player" && this.shooter.Equals("player") || collision.tag == "Enemy" && this.shooter.Equals("enemy");
        bool bulletsHitsBullet = collision.tag == "Bullet";
        bool aCharacterWasHit = collision.tag == "Player" || collision.tag == "Enemy";
        // don't destroy the bullet if it collides with the same one who shoot it &&
        // don't destroy the bulet if it collides with another bullet &&
        // someone loses life if previous conditions are respected
        if (!bulletsHitsBullet && !shooterAndHitAreTheSame) 
        {
            if (aCharacterWasHit)
            {
                LifeCycle hit = collision.GetComponent<LifeCycle>();
                hit.TakeDamage(getDamageAmount());
            }
            Destroy(gameObject);
        }
    }

    internal void setShooter(string shooter)
    {
        this.shooter = shooter;
    }
}
