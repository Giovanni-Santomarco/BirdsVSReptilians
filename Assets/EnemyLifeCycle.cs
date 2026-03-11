using UnityEngine;

public class EnemyLifeCycle : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    internal BoardManager levelManager;
    public GameObject Rapace_morto;

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

    void Die()
    {
        // Add death animation or sound here TODO

        // destroy the object
        Transform spotOfDeath = gameObject.transform;

        Destroy(gameObject);

        if(Rapace_morto != null)
        {
            Instantiate(Rapace_morto, spotOfDeath.position, spotOfDeath.rotation);
        }

        //tell current level about the death, a death triggers a check by LevelManager, i.e. isNewLevel?
        levelManager.decreaseEnemies();
    }
}
