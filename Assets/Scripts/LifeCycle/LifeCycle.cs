using UnityEngine;

public abstract class LifeCycle : MonoBehaviour
{
    public int maxHealth = 100;
    protected int currentHealth;
    internal LevelManager levelManager;
    internal DamageFeedback damageFeedback;

    void Start()
    {
        currentHealth = maxHealth;

        damageFeedback = GetComponent<DamageFeedback>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }

        damageFeedback.StartHitAnimation();
    }

    public abstract void Die();
}
