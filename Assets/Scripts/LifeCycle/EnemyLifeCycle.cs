using UnityEngine;

public class EnemyLifeCycle : LifeCycle
{
    public GameObject Rapace_morto;

    public GameObject medikit;

    public GameObject weaponHolder;

    public float chanceToDropAMedikit = 0.05f;
    public float chanceToDropAGun = 0.07f;


    public override void Die()
    {
        // destroy the object
        Transform spotOfDeath = gameObject.transform;

        GameObject weaponPrefabToDrop = weaponHolder.transform.GetChild(0).GetComponent<WeaponInfo>().pickupPrefab;

        Destroy(gameObject);

        float roll = Random.value;

        if (roll < chanceToDropAMedikit)
        {
            //tell the current level to spawn a medikit
            levelManager.SpawnDrop(medikit, spotOfDeath);
        }
        else if(roll < chanceToDropAMedikit + chanceToDropAGun)
        {
            //tell the current level to spawn the enemy's gun
            levelManager.SpawnDrop(weaponPrefabToDrop, spotOfDeath);
        }
        else
        {
            //tell the current level to spawn a dead enemy at the spot in which its health went down to 0
            levelManager.SpawnDrop(Rapace_morto, spotOfDeath);
        }


        //tell current level about the death, a death triggers a check by LevelManager, i.e. isNewLevel?
        levelManager.decreaseEnemies();
    }
}
