using UnityEngine;
using UnityEngine.UI; // Required for the Slider component

public class HealthBarManager : MonoBehaviour
{
    public Slider healthSlider;
    public LifeCycle playerLife;

    void Update()
    {
        if (playerLife != null)
        {
            // Update the slider value every frame based on player health
            healthSlider.value = playerLife.GetLifePercentage();
        }
    }
}