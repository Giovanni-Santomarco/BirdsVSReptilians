using UnityEngine;

public class GoodAimShootManager : ShootManagerForEnemy
{
    public LayerMask obstacleLayer;      // I layer dei muri (per fermare il laser se colpisce un ostacolo)
    public LayerMask playerLayer;
    public float laserDistance = 20f;    // Quanto è lungo il raggio


    protected override void ShootingMechanics()
    {
        if (canShoot() && IsAimingWell())
        {
            //tries to shoot, but i can also occur that it does not happen -> watch GunController to see the reason (if nextShotTime of the gun did not pass (the rate of fire))
            if (gunController.Shoot())
            {
                //i count that a bullet has been shoot
                bulletCounter++;

            }

            //if i finished all my bullets i need to reload
            if (bulletCounter >= magazine)
            {
                Reload();
            }
        }
    }

    private bool IsAimingWell()
    {
        Vector2 aimDirection = gunController.firePoint.right;
        if (gunController.firePoint.lossyScale.x < 0)
        {
            aimDirection = -gunController.firePoint.right;
        }

        // il raggio può sbattere solo contro i muri E contro il Player (gli altri layer non verranno considerati)
        int checkLayers = obstacleLayer | playerLayer;

        //Lanciamo il raggio invisibile dalla canna dell'arma
        RaycastHit2D hit = Physics2D.Raycast(gunController.firePoint.position, aimDirection, laserDistance, checkLayers);

        // se il raggio colpisce qualcosa e quel qualcosa è il giocatore
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            return true;
        }

        // Se il raggio colpisce un muro prima del giocatore, o non colpisce niente (è girato male), ritorniamo falso.
        return false;
    }
}
