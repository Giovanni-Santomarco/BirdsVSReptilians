using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Transform weaponHolder;

    [Header("Arma iniziale")]
    public GameObject startingWeapon;

    public GameObject[] slots = new GameObject[2];  //quante armi posso avere in mano
    private int currentSlotIndex = 0;    //arma che ho in mano attualmente

    private LevelManager levelManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();

        if (startingWeapon != null)
        {
            PickupWeapon(startingWeapon, transform);
        } 
    }


    public void PickupWeapon(GameObject weapon, Transform positionWeaponOnTheGround, int ammoToTransfer = -1)
    {
        //scopriamo che tipo di arma stiamo raccogliendo: se raccolgo una sideArm avrà colpi infiniti altrimenti se raccolgo una primary allora avrà un certo numero di colpi
        WeaponInfo info = weapon.GetComponent<WeaponInfo>();
        if (info == null) return;

        //decidiamo in quale slot deve andare: se è Sidearm va nello slot 0, altrimenti nello slot 1
        int targetSlot = (info.category == WeaponCategory.Sidearm) ? 0 : 1;

        if (slots[targetSlot] != null)
        {
            //viene droppata l'arma nel caso in cui lo slot in cui si vuole mettere l'arma è già occupato da un altra arma
            DropWeapon(targetSlot, positionWeaponOnTheGround.position);
        }

        //equipaggiamo la nuova arma nel suo slot dedicato e la mettiamo in mano
        EquipWeapon(weapon, targetSlot, ammoToTransfer);
        SwitchToSlot(targetSlot);
    }


    
    void DropWeapon(int indexSlot, Vector3 positionWeaponOnTheGround)
    {
        GameObject weaponToDrop = slots[indexSlot];
        if (weaponToDrop != null)
        {
            WeaponInfo info = weaponToDrop.GetComponent<WeaponInfo>();
            GunController gunCtrl = weaponToDrop.GetComponent<GunController>();

            if (info != null && info.pickupPrefab != null)
            {
                if (levelManager != null)
                {
                    GameObject droppedItem = levelManager.SpawnDropGun(info.pickupPrefab, positionWeaponOnTheGround);   //ricorda SpawnDropGun ritorna un riferimento all'arma

                    //TRASFERIAMO I COLPI ALL'OGGETTO A TERRA
                    if (droppedItem != null && gunCtrl != null)
                    {
                        WeaponPickup pickupScript = droppedItem.GetComponent<WeaponPickup>();
                        if (pickupScript != null)
                        {
                            pickupScript.savedAmmo = gunCtrl.currentAmmo;
                        }
                    }
                }
                else  //nel caso qualcosa vada storto
                {
                    Instantiate(info.pickupPrefab, positionWeaponOnTheGround, Quaternion.identity);
                }
            }
            Destroy(weaponToDrop);
            slots[indexSlot] = null;
        }
    }


    void EquipWeapon(GameObject weapon, int indexSlot, int ammoToTransfer)
    {
        GameObject newWeapon = Instantiate(weapon, weaponHolder);
        GunController gunCtrl = newWeapon.GetComponent<GunController>();
        gunCtrl.setShooter("player");
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        //se l'arma NON è nuova (-1), sovrascriviamo i suoi proiettili
        if (ammoToTransfer != -1)
        {
            gunCtrl.currentAmmo = ammoToTransfer;
        }

        slots[indexSlot] = newWeapon;
    }


    void SwitchToSlot(int indexSlot)
    {
        //se l'indice è quello dell'arma vecchia non faccio nulla
        if (indexSlot == currentSlotIndex) return;

        //se lo slot selezionato non contiene armi
        if (slots[indexSlot] == null) return;

        //metto "in inventario" l'arma che avevo prima in mano
        if (slots[currentSlotIndex] != null)
        {
            slots[currentSlotIndex].SetActive(false);
        }

        currentSlotIndex = indexSlot;

        //metto in mano l'arma che avevo nell'inventario
        if (slots[currentSlotIndex] != null)
        {
            slots[currentSlotIndex].SetActive(true);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToSlot(1);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f) // Se giro in SU -> Prossima arma
        {
            int nextSlot = currentSlotIndex + 1;

            // Se supero il limite, torno al primo (0)
            if (nextSlot >= slots.Length)
            {
                nextSlot = 0;
            }

            SwitchToSlot(nextSlot);
        }
        else if (scroll < 0f) // Se giro in GIÙ -> Arma precedente
        {
            int prevSlot = currentSlotIndex - 1;

            // Se vado sotto zero, vado all'ultimo slot disponibile
            if (prevSlot < 0)
            {
                prevSlot = slots.Length - 1;
            }

            SwitchToSlot(prevSlot);
        }
    }

    public GameObject getCurrentWeapon()
    {
        return this.slots[currentSlotIndex];
    }


    // NUOVA FUNZIONE: Viene chiamata dal GunController quando finiscono i colpi -> l'arma viene distrutta se i colpi finiscono
    public void BreakCurrentWeapon()
    {
        if (slots[currentSlotIndex] != null)
        {
            // Distruggiamo l'arma
            Destroy(slots[currentSlotIndex]);
            slots[currentSlotIndex] = null;

            // Cambiamo automaticamente all'altra arma rimasta
            int otherSlot = (currentSlotIndex == 0) ? 1 : 0;
            SwitchToSlot(otherSlot);
        }
    }
}
