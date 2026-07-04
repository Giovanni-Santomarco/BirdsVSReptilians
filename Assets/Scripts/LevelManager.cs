using UnityEngine;
using NavMeshPlus.Components;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    //container
    private GameObject boardHolder; // Reference to the current level container
    
    public GameObject navMeshManager;          

    internal GameObject getBoardHolder()
    {
        return this.boardHolder;
    }

    public GameObject player;
    public EnemySpawner enemySpawner;
    public MapManager mapManager;
    public GameManager gameManager;
    private Vector2 playerPos;

    private int nEnemies;

    private int nBosses;

    public void cleanLevel()
    {
        if (boardHolder != null)
        {
            // sposto la vecchia mappa a 10.000 coordinate di distanza.
            // Spostando il "padre", tutti i muri vecchi lo seguiranno all'istante.
            boardHolder.transform.position = new Vector3(10000f, 10000f, 0f);

            // 2. La spegniamo
            boardHolder.SetActive(false);

            // 3. Sincronizziamo i calcoli di Unity FORZATAMENTE in questo esatto millisecondo
            Physics2D.SyncTransforms();

            Destroy(boardHolder);
        }
        navMeshManager.GetComponent<NavMeshSurface>().RemoveData();
    }

    public IEnumerator GenerateLevel(int level)
    {
        //regarding cleaning
        cleanLevel();
        boardHolder = new GameObject("BoardHolder");
        //map
        mapManager.generateMap(level);

        yield return null;

        Physics2D.SyncTransforms();

        //per il pathfinding: se devi modificare mantieni questa riga sopra lo spawn di nemici
        if (navMeshManager != null)
        {
            navMeshManager.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
        else
        {
            Debug.LogError("problems");
        }

        yield return null;

        //characters
        SpawnPlayer();
        enemySpawner.SpawnEnemiesForLevel(level);
    }

    void SpawnPlayer()
    {
        playerPos = mapManager.getRandomFreeTaleNormalized();
        player.transform.position = playerPos;
        if (player.GetComponent<PlayerLifeCycle>().levelManager == null)
        {
            player.GetComponent<PlayerLifeCycle>().levelManager = this;
        }
        //player.GetComponent<PlayerLifeCycle>().resetLife(); il player non deve recuperare tutta la vita a fine livello
    }

    public void SpawnDrop(GameObject objectToDrop, Transform spotOfDrop)
    {
        if (objectToDrop != null && boardHolder != null)
        {
            GameObject drop = Instantiate(objectToDrop, spotOfDrop.position, spotOfDrop.rotation);
            drop.transform.SetParent(boardHolder.transform);
        }
    }


    public GameObject SpawnDropGun(GameObject objectToDrop, Vector3 spotOfDrop)
    {
        if (objectToDrop != null && boardHolder != null)
        {
            GameObject newItem = Instantiate(objectToDrop, spotOfDrop, Quaternion.identity);
            newItem.transform.SetParent(boardHolder.transform);

            return newItem; //mi serve il return per capire poi quanti proiettili assegnare all'arma
        }
        return null;
    }

    internal void decreaseEnemies()
    {
        nEnemies--;
        if (nEnemies <= 0)
        {
            gameManager.noEnemiesInCurrentLevel();
        }
    }

    internal void decreaseBosses()
    {
        nBosses--;
        if(nBosses <= 0)
        {
            InventoryManager playerInventory = FindFirstObjectByType<InventoryManager>();
            if (playerInventory != null)
            {
                playerInventory.UpgradeSidearm();
            }
        }
    }

    internal void playerDies()
    {
        gameManager.playerIsDead();
    }

    internal void setNenemies(int targetEnemyCount)
    {
        this.nEnemies = targetEnemyCount;
    }

    public void setNBosses(int targetBossCount)
    {
        this.nBosses = targetBossCount;
    }
}