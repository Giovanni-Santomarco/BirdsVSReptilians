using UnityEngine;

//this function instantiates the gun of an ENEMY, the choose of the gun is done runtime.
//it is simple to custom this class to randomly choose a gun between many.
public class SingleGunManager : MonoBehaviour

{
    public GameObject weapon;
    public Transform weaponHolder;
    void Start()
    {
        if (weapon != null)
        {
            GameObject newWeapon = Instantiate(weapon, weaponHolder);
            newWeapon.GetComponent<GunController>().setShooter("enemy");
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localRotation = Quaternion.identity;
            weapon = newWeapon;
        }
    }
}
