using UnityEngine;

public class MedikitScript : MonoBehaviour
{
    [Header("how much life it does bring back to the player")]
    public int lifeHeal = 50;
    [Header("how much time it takes to expire")]
    public float timeToLive = 10f;

    void Start()
    {
        //it makes the duration of this game object at timeToLive -> passed 10 sec it will be destroyed
        Destroy(gameObject, timeToLive);
    }

    
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (gameObject != null && collider.CompareTag("Player"))
        {
            PlayerLifeCycle playerLifeCycle = collider.GetComponent<PlayerLifeCycle>();

            if(playerLifeCycle != null)
            {
                playerLifeCycle.Heal(lifeHeal);
            }

            Destroy(gameObject);
        }
    }
}
