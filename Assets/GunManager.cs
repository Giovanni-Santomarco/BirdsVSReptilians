using UnityEngine;

public class SingleGunManager : MonoBehaviour

{
    public GameObject weapon;
    public Transform weaponHolder;
    void Start()
    {
        if (weapon != null)
        {
            GameObject newWeapon = Instantiate(weapon, weaponHolder);
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localRotation = Quaternion.identity;
            weapon = newWeapon;
        }
    }
}
