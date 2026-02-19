using UnityEngine;

public class BulletOfPistol : Bullet
{
    public int damage = 50;
    public float speed = 5.0f;
    public Rigidbody2D rigidBody;
    public override int getDamageAmount()
    {
        return damage;
    }

    public override Rigidbody2D getRigidBody()
    {
        return rigidBody;
    }

    public override float getSpeed()
    {
        return speed;
    }
}
