using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLifeCycle : LifeCycle
{
    public GameObject playerDead;

    public override void Die()
    {
        Transform spotOfDeath = gameObject.transform;

        Destroy(gameObject);

        levelManager.SpawnDrop(playerDead, spotOfDeath);

        levelManager.playerDies();
    }

    internal void resetLife()
    {      
        this.currentHealth = maxHealth;  
    }

    public void Heal(int heal)
    {
        if (currentHealth + heal > maxHealth)
        {
            currentHealth = maxHealth;
            return;
        }
        currentHealth += heal;

        feedback.StartHealAnimation();
    }
}
