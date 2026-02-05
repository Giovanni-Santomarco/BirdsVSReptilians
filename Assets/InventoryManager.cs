using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Transform weaponHolder;

    [Header("Arma iniziale")]
    public GameObject startingWeapon;

    public GameObject[] slots = new GameObject[2];  //quante armi posso avere in mano
    public int currentSlotIndex = 0;    //arma che ho in mano attualmente

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(startingWeapon != null)
        {
            PickupWeapon(startingWeapon);
        } 
    }


    void PickupWeapon(GameObject weapon)
    {
        int emptySlotIndex = -1;
        for(int i=0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                emptySlotIndex = i;
                break;
            }
        }

        //se abbiamo già tutti gli slot pieni devo droppare la mia arma attuale
        if(emptySlotIndex == -1)
        {
            DropCurrentWeapon();
            EquipWeapon(weapon, currentSlotIndex);
        }
        else
        {
            EquipWeapon(weapon, emptySlotIndex);
            SwitchToSlot(emptySlotIndex);
        }
    }


    void DropCurrentWeapon()
    {
        GameObject currentWeapon = slots[currentSlotIndex];
        if (currentWeapon != null)
        {
            WeaponInfo info = currentWeapon.GetComponent<WeaponInfo>();
            if (info != null && info.pickupPrefab != null)
            {
                Instantiate(info.pickupPrefab, transform.position, Quaternion.identity);
            }
            Destroy(currentWeapon);
            slots[currentSlotIndex] = null;
        }
    }


    void EquipWeapon(GameObject weapon, int indexSlot)
    {
        GameObject newWeapon = Instantiate(weapon, weaponHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        slots[indexSlot] = newWeapon;
    }


    void SwitchToSlot(int indexSlot)
    {
        //se l'indice è quello dell'arma vecchia non faccio nulla
        if (indexSlot == currentSlotIndex) return;

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
        if (Input.GetKeyDown(KeyCode.WheelUp) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.WheelDown) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToSlot(1);
        }
    }
}
