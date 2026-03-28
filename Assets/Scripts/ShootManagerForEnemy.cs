using UnityEngine;

public class ShootManagerForEnemy : MonoBehaviour
{
    public SingleGunManager gunManager; 
    public Enemy_Movement enemyMovement; // I need this because it stores enemy's status (e.g. fight mode, patroling)
                                         // fight mode ==> shoot
    private GunController gunController;

    // Update is called once per frame
    void Update()
    {
        if (gunController == null)
        {
            gunController = gunManager.weapon.GetComponent<GunController>();
        }
        if (enemyMovement.getCurrentState() == Enemy_Movement.EnemyState.Combat && enemyMovement.hasLineOfSight())  //the enemy has to see the enmy in order to shoot
        {
            gunController.Shoot();
        }
    }
}
