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
        // if the bullet hits an enemy (or the player, TODO), enemy (or player, TODO) loses hp

        //need to change the following line!
        if (collision.tag == "Enemy" && !this.shooter.Equals("enemy"))
        {
            EnemyLifeCycle enemy = collision.GetComponent<EnemyLifeCycle>();
            if (enemy != null) {
                enemy.TakeDamage(getDamageAmount());
            }
        }
        // destroy the bullet
        if (collision.tag != "Player")
        {
            Destroy(gameObject);
        }
    }

    internal void setShooter(string shooter)
    {
        this.shooter = shooter;
    }
}
