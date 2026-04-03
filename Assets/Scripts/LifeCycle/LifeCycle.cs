using UnityEngine;

public abstract class LifeCycle : MonoBehaviour
{
    public int maxHealth = 100;
    protected int currentHealth;
    internal LevelManager levelManager;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public abstract void Die();
}
