using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Transform weaponHolder;

    [Header("Arma iniziale")]
    public GameObject startingWeapon;

    public GameObject[] slots = new GameObject[2];  //quante armi posso avere in mano
    private int currentSlotIndex = 0;    //arma che ho in mano attualmente

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(startingWeapon != null)
        {
            PickupWeapon(startingWeapon, transform.position);
        } 
    }


    public void PickupWeapon(GameObject weapon, Vector3 positionWeaponOnTheGround)
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
            DropCurrentWeapon(positionWeaponOnTheGround); // does not effect currentSlotIndex
            EquipWeapon(weapon, currentSlotIndex);        // does not effect currentSlotIndex
        }
        else
        {
            EquipWeapon(weapon, emptySlotIndex);          
            SwitchToSlot(emptySlotIndex);                 //effects currentSlotIndex
        }
    }


    void DropCurrentWeapon(Vector3 positionWeaponOnTheGround)
    {
        GameObject currentWeapon = slots[currentSlotIndex];
        if (currentWeapon != null)
        {
            WeaponInfo info = currentWeapon.GetComponent<WeaponInfo>();
            if (info != null && info.pickupPrefab != null)
            {
                Instantiate(info.pickupPrefab, positionWeaponOnTheGround, Quaternion.identity);
            }
            Destroy(currentWeapon);
            slots[currentSlotIndex] = null;
        }
    }


    void EquipWeapon(GameObject weapon, int indexSlot)
    {
        weapon.GetComponent<GunController>().setShooter("player");
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
}
