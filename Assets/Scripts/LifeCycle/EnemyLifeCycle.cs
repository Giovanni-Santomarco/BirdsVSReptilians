using UnityEngine;

public class EnemyLifeCycle : LifeCycle
{
    public GameObject Rapace_morto;
    public override void Die()
    {
        // destroy the object
        Transform spotOfDeath = gameObject.transform;

        Destroy(gameObject);

        if (Rapace_morto != null)
        {
            Instantiate(Rapace_morto, spotOfDeath.position, spotOfDeath.rotation);
        }

        //tell current level about the death, a death triggers a check by LevelManager, i.e. isNewLevel?
        levelManager.decreaseEnemies();
    }
}
