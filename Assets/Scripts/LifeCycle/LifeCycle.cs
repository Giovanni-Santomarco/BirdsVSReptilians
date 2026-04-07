using UnityEngine;

public abstract class LifeCycle : MonoBehaviour
{
    public int maxHealth = 100;
    protected int currentHealth;
    internal LevelManager levelManager;
    internal HealAndDamageFeedback feedback;

    void Start()
    {
        currentHealth = maxHealth;

        feedback = GetComponent<HealAndDamageFeedback>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }

        feedback.StartHitAnimation();
    }

    public abstract void Die();
}
