using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using NavMeshPlus.Components;

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
            Physics.SyncTransforms();

            Destroy(boardHolder);
        }
        navMeshManager.GetComponent<NavMeshSurface>().RemoveData();
    }

    public void GenerateLevel(int level)
    {
        //regarding cleaning
        cleanLevel();
        boardHolder = new GameObject("BoardHolder");
        //map
        mapManager.generateMap(level);

        //per il pathfinding: se devi modificare mantieni questa riga sopra lo spawn di nemici
        if (navMeshManager != null)
        {
            navMeshManager.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
        else
        {
            Debug.LogError("problems");
        }

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
        player.GetComponent<PlayerLifeCycle>().resetLife();
    }

    public void SpawnDeathEnemy(GameObject Rapace_morto, Transform spotOfDeath)
    {
        if (Rapace_morto != null && boardHolder != null)
        {
            GameObject deadBody = Instantiate(Rapace_morto, spotOfDeath.position, spotOfDeath.rotation);
            deadBody.transform.SetParent(boardHolder.transform);
        }
    }

    internal void decreaseEnemies()
    {
        nEnemies--;
        if (nEnemies <= 0)
        {
            gameManager.noEnemiesInCurrentLevel();
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
}