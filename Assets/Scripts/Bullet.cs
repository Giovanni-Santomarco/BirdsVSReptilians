using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    //public Rigidbody2D rb;
    //public float speed = 2.0f;
    //public int damageAmount = 25;

    abstract public int getDamageAmount();
    abstract public Rigidbody2D getRigidBody();
    abstract public float getSpeed();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        getRigidBody().linearVelocity = getSpeed() * transform.right;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if the bullet hits an enemy (or the player, TODO), enemy (or player, TODO) loses hp
        if (collision.tag == "Enemy")
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
}
