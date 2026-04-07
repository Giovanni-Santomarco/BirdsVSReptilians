using UnityEngine;

public class EnemyLifeCycleOld : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    internal LevelManager levelManager;
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

        //tell the current level to spawn a dead enemy at the spot in which its health went down to 0
        levelManager.SpawnDrop(Rapace_morto, spotOfDeath);

        //tell current level about the death, a death triggers a check by LevelManager, i.e. isNewLevel?
        levelManager.decreaseEnemies();
    }
}
