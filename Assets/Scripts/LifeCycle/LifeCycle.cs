using System;
using UnityEngine;

public abstract class LifeCycle : MonoBehaviour
{
    public int maxHealth = 100;
    protected int currentHealth;
    internal LevelManager levelManager;
    internal HealAndDamageFeedback feedback;

    protected bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        feedback = GetComponent<HealAndDamageFeedback>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
        else
        {
            feedback.StartHitAnimation();
        }
    }

    public abstract void Die();

    internal float GetLifePercentage()
    {
        return 1f - ((float)this.currentHealth / (float)this.maxHealth);
    }
}
