using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLifeCycle : LifeCycle
{
    public override void Die()
    {
        levelManager.playerDies();
    }

    internal void resetLife()
    {      
        this.currentHealth = maxHealth;  
    }
}
