using UnityEngine;

public class EnemyLifeCycle : LifeCycle
{
    public GameObject Rapace_morto;
    public override void Die()
    {
        // destroy the object
        Transform spotOfDeath = gameObject.transform;

        Destroy(gameObject);

        //tell the current level to spawn a dead enemy at the spot in which its health went down to 0
        levelManager.SpawnDeathEnemy(Rapace_morto, spotOfDeath);

        //tell current level about the death, a death triggers a check by LevelManager, i.e. isNewLevel?
        levelManager.decreaseEnemies();
    }
}
