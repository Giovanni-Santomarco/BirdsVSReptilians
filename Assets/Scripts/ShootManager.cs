using UnityEngine;
using static UnityEngine.Android.AndroidGame;

public class ShootManager : MonoBehaviour
{
    public InventoryManager inventoryManagerOfPlayer;
    private GunController gunControllerOfCurrentWeapon;

    private GameObject lastKnownWeapon;  //to remember last known weapon

    // Update is called once per frame
    void Update()
    {
        // If Time.timeScale is 0, the game is paused. Don't shoot!
        if (Time.timeScale == 0) return;

        //se non aggiorniamo anche il riferimento in ShootManager quello che avverrà è che cambio l'arma ma 
        //lo shootManager proverà a sparare con l'arma vecchia -> spara con l'arma vecchia nonostante abbiamo impostato l'arma nuova ma non riprodurrà il suo audio dato che l'arma 
        //in quel momento non è active

        // we ask ourself each time which gun do we have?
        GameObject currentWeapon = inventoryManagerOfPlayer.getCurrentWeapon();

        if (currentWeapon != lastKnownWeapon)
        {
            lastKnownWeapon = currentWeapon;

            if (currentWeapon != null)
            {
                gunControllerOfCurrentWeapon = currentWeapon.GetComponent<GunController>();
            }
            else
            {
                //if there are no guns
                gunControllerOfCurrentWeapon = null;
            }
        }

        if (gunControllerOfCurrentWeapon == null) return;

        bool shootInput = false;
        if (gunControllerOfCurrentWeapon.isAutomatic)
        {
            shootInput = Input.GetButton("Fire1");
        }
        else
        {
            shootInput = Input.GetMouseButtonDown(0);
        }

        if (shootInput)
        {
            //shoot fun takes into acconuting fire rate, i.e. shoots only if fire rate allows to
            gunControllerOfCurrentWeapon.Shoot();
        }

    }
}
