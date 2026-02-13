using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth;
    public int gridHeight;
    public int maxFloorCount; // How many floor tiles to create
    //at the moment, MUST be 2
    public float tileSize = 2f;

    [Header("Generation Parameters")]
    [Range(0f, 1f)] public float chanceToChangeDir = 0.5f;
    [Range(0f, 1f)] public float chanceToSpawnWalker = 0.05f;
    [Range(0f, 1f)] public float chanceToDestroyWalker = 0.05f;

    [Header("References")]
    public GameObject[] floorPrefab;
    public GameObject[] nonPhysWallPrefab;
    public GameObject[] physWallPrefab;

    // 0 = Wall, 1 = Floor
    private int[,] gridData;
    private List<Vector2> freeTales = new List<Vector2>();

    // Simple class to track our miners
    private class Walker
    {
        public Vector2Int position;
        public Vector2Int direction;
    }

    private List<Walker> walkers;

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        //map
        SetupGrid();
        RunWalkers();
        SpawnGeometry();
        //characters
        SpawnPlayer();
    }

    //map region

    void SetupGrid()
    {
        gridData = new int[gridWidth, gridHeight];

        // Initialize everything as a Wall (0)
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                gridData[x, y] = 0;
            }
        }
    }

    void RunWalkers()
    {
        // Reset walkers list
        walkers = new List<Walker>();

        // Create the first walker at the center
        Walker firstWalker = new Walker();
        firstWalker.direction = RandomDirection();
        firstWalker.position = new Vector2Int(gridWidth / 2, gridHeight / 2);
        walkers.Add(firstWalker);

        int currentFloors = 0;

        // THE MAIN LOOP
        // Keep digging until we reach our target floor count
        while (currentFloors < maxFloorCount)
        {
            // Iterate backwards so we can remove walkers safely
            for (int i = walkers.Count - 1; i >= 0; i--)
            {
                //Walker thisWalker = walkers[i];

                // 1. Move
                walkers[i].position += walkers[i].direction;

                // 2. Clamp position to grid bounds (leave 1 tile padding for walls)
                walkers[i].position.x = Mathf.Clamp(walkers[i].position.x, 1, gridWidth - 2);
                walkers[i].position.y = Mathf.Clamp(walkers[i].position.y, 1, gridHeight - 2);

                // 3. Carve Floor
                if (gridData[walkers[i].position.x, walkers[i].position.y] != 1)
                {
                    gridData[walkers[i].position.x, walkers[i].position.y] = 1;
                    currentFloors++;
                    //keep track of free positions
                    freeTales.Add(new Vector2(walkers[i].position.x, walkers[i].position.y));
                }

                // 4. Change Direction?
                if (Random.value < chanceToChangeDir)
                {
                    walkers[i].direction = RandomDirection();
                }

                // 5. Spawn a NEW Walker? (Branching)
                if (Random.value < chanceToSpawnWalker)
                {
                    Walker newWalker = new Walker();
                    newWalker.position = walkers[i].position;
                    newWalker.direction = RandomDirection();
                    walkers.Add(newWalker);
                }

                // 6. Destroy this Walker? (Merging paths)
                // Only destroy if we have more than 1 walker to prevent extinction
                if (Random.value < chanceToDestroyWalker && walkers.Count > 1)
                {
                    walkers.RemoveAt(i);
                }

                // Safety break if we hit the limit mid-loop
                if (currentFloors >= maxFloorCount) break;
            }
        }
    }

    void SpawnGeometry()
    {
        int normalizerForX = gridWidth / 2;
        int normalizerForY = gridHeight / 2;
        // Simple loop to instantiate prefabs based on grid data
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 pos = new Vector2((x - normalizerForX) * tileSize, (y - normalizerForY) * tileSize);

                if (gridData[x, y] == 1) // Floor
                {
                    //choose a random floor tale
                    int randomInt = Random.Range(0, floorPrefab.Length);
                    Instantiate(floorPrefab[randomInt], pos, Quaternion.identity);
                }
                else // Wall
                {
                    // Optimization: Only spawn physics walls if they touch a floor
                    if (HasFloorNeighbor(x, y))
                    {
                        int r = Random.Range(0, physWallPrefab.Length);
                        Instantiate(physWallPrefab[r], pos, Quaternion.identity);
                    }
                    else
                    {
                        int r = Random.Range(0, nonPhysWallPrefab.Length);
                        Instantiate(nonPhysWallPrefab[r], pos, Quaternion.identity);
                    }
                }
            }
        }
    }

    // Helper to check neighbors
    bool HasFloorNeighbor(int x, int y)
    {
        // check 4 cardinal directions
        if (x > 0 && gridData[x - 1, y] == 1) return true;
        if (x < gridWidth - 1 && gridData[x + 1, y] == 1) return true;
        if (y > 0 && gridData[x, y - 1] == 1) return true;
        if (y < gridHeight - 1 && gridData[x, y + 1] == 1) return true;
        return false;
    }

    Vector2Int RandomDirection()
    {
        int choice = Random.Range(0, 4);
        switch (choice)
        {
            case 0: return Vector2Int.up;
            case 1: return Vector2Int.down;
            case 2: return Vector2Int.left;
            default: return Vector2Int.right;
        }
    }

    //characters region

    public GameObject player;
    public Vector2 getRandomFreeTale()
    {
        int randomInt = Random.Range(0, freeTales.Count);
        Vector2 randomFreeTale = freeTales[randomInt];
        freeTales.RemoveAt(randomInt);
        return randomFreeTale;
    }

    void SpawnPlayer()
    {
        int normalizerForX = gridWidth / 2;
        int normalizerForY = gridHeight / 2;
        Vector2 startPos = getRandomFreeTale();
        Vector2 startPosNormalaizaed = new Vector2((startPos.x - normalizerForX) * tileSize, (startPos.y - normalizerForY) * tileSize);
        player.transform.position = startPosNormalaizaed;
    }
}