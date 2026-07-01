using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarManager : MonoBehaviour
{
    [Header("Salute")]
    public Slider healthSlider;
    public PlayerLifeCycle playerLife;

    [Header("Testi UI")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI ammoText;

    [Header("Manager di Gioco")]
    public GameManager gameManager;
    public InventoryManager playerInventory;

    void Update()
    {
        // 1. AGGIORNA LA VITA
        if (playerLife != null && healthSlider != null)
        {
            healthSlider.value = playerLife.GetLifePercentage();
        }

        // 2. AGGIORNA IL ROUND
        if (gameManager != null && roundText != null)
        {
            roundText.text = "ROUND " + gameManager.GetCurrentLevel();
        }

        // 3. AGGIORNA LE MUNIZIONI
        if (playerInventory != null && ammoText != null)
        {
            GameObject currentWeapon = playerInventory.getCurrentWeapon();

            if (currentWeapon != null)
            {
                GunController gun = currentWeapon.GetComponent<GunController>();
                WeaponInfo info = currentWeapon.GetComponent<WeaponInfo>();

                if (gun != null && info != null)
                {
                    if (info.category == WeaponCategory.Sidearm)
                    {
                        ammoText.text = "∞";
                    }
                    else
                    {
                        ammoText.text = gun.currentAmmo.ToString();
                    }
                }
            }
            else
            {
                ammoText.text = "-";
            }
        }
    }
}