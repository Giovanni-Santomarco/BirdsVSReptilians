using TMPro;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Configurazione")]
    public GameObject weaponToEquip; // Il prefab dell'arma vera
    public string nomeArma = "nomeArmaDaAssegnare";

    [Header("Interfaccia")]
    public TextMeshPro messageText; //per dire all'utente che se preme E potrà raccogliere l'arma


    private bool canPickup = false;
    private InventoryManager playerInventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //per sicurezza in caso mi scordi di inserire la scritta
        if (messageText != null)
        {
            messageText.text = "E to gather " + nomeArma;

            messageText.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(canPickup && Input.GetKeyDown(KeyCode.E))
        {
            if(playerInventory != null)
            {
                playerInventory.PickupWeapon(weaponToEquip, transform.position);
                Destroy(gameObject);    //gameObject si intende l'oggetto in cui si trova lo script da non confondere con GameObject
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            canPickup = true;

            //vedo com'è l'inventario del player
            playerInventory = collision.GetComponent<InventoryManager>();

            //faccio vedere la scritta quando sono all'interno della collision dell'arma
            if (messageText != null) messageText.gameObject.SetActive(true);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            canPickup = false;

            playerInventory = null;

            //disattivo la scritta
            if (messageText != null) messageText.gameObject.SetActive(false);
        }
    }



}
