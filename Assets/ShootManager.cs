using UnityEngine;
using static UnityEngine.Android.AndroidGame;

public class ShootManager : MonoBehaviour
{
    public InventoryManager inventoryManagerOfPlayer;
    private GunController gunControllerOfCurrentWeapon;

    // Update is called once per frame
    void Update()
    {
        // If Time.timeScale is 0, the game is paused. Don't shoot!
        if (Time.timeScale == 0) return;

        if (gunControllerOfCurrentWeapon == null)
        {
            gunControllerOfCurrentWeapon = inventoryManagerOfPlayer.getCurrentWeapon().GetComponent<GunController>();
        }

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
