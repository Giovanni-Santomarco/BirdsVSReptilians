using UnityEngine;

public class BulletOfAk47 : Bullet
{
    public int damage = 25;
    public float speed = 2.0f;
    public Rigidbody2D rigidBody;
    public override int getDamageAmount()
    {
        throw new System.NotImplementedException();
    }

    public override Rigidbody2D getRigidBody()
    {
        throw new System.NotImplementedException();
    }

    public override float getSpeed()
    {
        throw new System.NotImplementedException();
    }
}
