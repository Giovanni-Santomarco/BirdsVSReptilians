using UnityEngine;

public class PlayerLifeCycle : LifeCycle
{
    public override void Die()
    {
        levelManager.playerDies();
    }
}
