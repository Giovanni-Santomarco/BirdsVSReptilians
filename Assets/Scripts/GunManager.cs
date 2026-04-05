using System.Collections.Generic;
using UnityEngine;

//this function instantiates the gun of an ENEMY, the choise of the gun is done runtime.
//it is simple to custom this class to randomly choose a gun between many.
public class SingleGunManager : MonoBehaviour

{
    public List<GameObject> weapons;
    public Transform weaponHolder;
    private GameObject weapon;
    

    void Awake()
    {
        if(weapons != null && weapons.Count > 0)
        {
            int indiceCasuale = Random.Range(0, weapons.Count);

            initWeapon(indiceCasuale);
        }
    }

    private void initWeapon(int i)
    {
        GameObject newWeapon = Instantiate(weapons[i], weaponHolder);
        newWeapon.GetComponent<GunController>().setShooter("enemy");
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        weapon = newWeapon;
    }

    public GameObject getWeapon()
    {
        return weapon;
    }
}
