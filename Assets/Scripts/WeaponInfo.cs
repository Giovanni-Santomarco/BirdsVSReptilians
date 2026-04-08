using UnityEngine;

public enum WeaponCategory
{
    Sidearm, // Arma secondaria, colpi infiniti -> Andrà nello Slot 0
    Primary  // Arma primaria, colpi limitati -> Andrà nello Slot 1
}

public class WeaponInfo : MonoBehaviour
{
    [Header("Quale oggetto buttare?")]
    //PREFAB dell'arma da terra
    public GameObject pickupPrefab;
    public WeaponCategory category;
}