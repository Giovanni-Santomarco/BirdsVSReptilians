using UnityEngine;

public class ShootManagerForEnemy : MonoBehaviour
{
    public SingleGunManager gunManager; 
    public Enemy_Movement enemyMovement; // I need this because it stores enemy's status (e.g. fight mode, patroling)
                                         // fight mode ==> shoot
    protected GunController gunController;

    [SerializeField]
    protected float reloadTime = 10f;
    protected float timeToWait = 0;

    [SerializeField]
    protected int magazine = 5;
    protected int bulletCounter = 0;

    protected virtual void Start()
    {
        if (gunManager != null && gunManager.weapon != null)
        {
            gunController = gunManager.weapon.GetComponent<GunController>();
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        ShootingMechanics();
    }

    protected virtual void ShootingMechanics()
    {
        if (canShoot())
        {
            //tries to shoot, but i can also occur that it does not happen -> watch GunController to see the reason (if nextShotTime of the gun did not pass (the rate of fire))
            if (gunController.Shoot())
            {
                //i count that a bullet has been shoot
                bulletCounter++;

            }

            //if i finished all my bullets i need to reload
            if(bulletCounter >= magazine)
            {
                Reload();
            }
        }
    }

    protected bool canShoot()
    {
        //the enemy has to see the enemy in order to shoot and, in the meantime, it has to be in combat mode
        return enemyMovement.getCurrentState() == Enemy_Movement.EnemyState.Combat && enemyMovement.hasLineOfSight() && !IsReloading();
    }

    protected bool IsReloading()
    {
        return Time.time < timeToWait;
    }

    protected void Reload()
    {
        bulletCounter = 0;
        timeToWait = Time.time + reloadTime;
    }
}
