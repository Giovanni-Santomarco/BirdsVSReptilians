using System.Collections;
using UnityEngine;

// Ereditiamo dalla tua classe base
public class TelegraphedShootManager : ShootManagerForEnemy
{
    [Header("Impostazioni Laser")]
    public LineRenderer laserLine;
    public float aimDuration = 1.5f;     // Quanti secondi mira prima di sparare
    public float laserDistance = 20f;    // Quanto è lungo il raggio
    public LayerMask obstacleLayer;      // I layer dei muri (per fermare il laser se colpisce un ostacolo)
    public LayerMask playerLayer;

    private bool isAiming = false;       // Evita che faccia partire più laser contemporaneamente

    private Marksman_Movement marksmanMovement;


    protected override void Start()
    {
        base.Start();

        marksmanMovement = enemyMovement as Marksman_Movement;
    }

    protected override void ShootingMechanics()
    {
        // Se ha i requisiti per sparare e NON sta già mirando
        if (canShoot() && !isAiming && IsAimingWell())
        {
            StartCoroutine(AimAndShootSequence());
        }
    }

    // nota: Una Coroutine ci permette di aspettare del tempo senza bloccare tutto il gioco
    private IEnumerator AimAndShootSequence()
    {
        isAiming = true;

        if(marksmanMovement != null)
        {
            marksmanMovement.setIsAimLocked(true);
        }

        // accendo il laser
        laserLine.enabled = true;

        float timer = 0f;

        // mantengo il laser acceso per la durata della aimduration
        while (timer < aimDuration)
        {
            // Continuiamo ad aggiornare la posizione del laser nel caso il nemico si muova o ruoti l'arma (forse farò modifiche a riguardo)
            UpdateLaserPosition();

            timer += Time.deltaTime;
            yield return null; // Aspetta il frame successivo prima di continuare il ciclo
        }

        // quando finisce il timer, spengo il laser e sparo
        laserLine.enabled = false;

        if (gunController.Shoot())
        {
            bulletCounter++;

            if (bulletCounter >= magazine)
            {
                Reload();
            }
        }
 
        yield return new WaitForSeconds(gunController.fireRate);

        isAiming = false;

        if (marksmanMovement != null)
        {
            marksmanMovement.setIsAimLocked(false);
        }

    }

    private void UpdateLaserPosition()
    {
        // indice 0 è il muzzle
        laserLine.SetPosition(0, gunController.firePoint.position);

        Vector2 laserDirection = gunController.firePoint.right;

        if (gunController.firePoint.lossyScale.x < 0)
        {
            laserDirection = -gunController.firePoint.right;
        }

        // 3. Usiamo la nostra nuova laserDirection corretta per il Raycast
        RaycastHit2D hit = Physics2D.Raycast(gunController.firePoint.position, laserDirection, laserDistance, obstacleLayer);

        if (hit.collider != null)
        {
            // Se sbatte sul muro, si ferma lì
            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            // Se non ci sono ostacoli, va dritto per la sua massima distanza
            laserLine.SetPosition(1, (Vector2)gunController.firePoint.position + (laserDirection * laserDistance));
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