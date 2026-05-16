using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{

    public LevelManager levelManager;

    [Header("Grid Settings")]
    //base cases
    public int baseCaseGridWidth;
    public int baseCaseGridHeight;
    [Header("from 0 to 1 notation, same for other percentages")]
    public float floorPercentage;
    //percentage increases
    public float widthIncreasePercentage;
    public float heightIncreasePercentage;
    //current level variables
    private int gridWidth;
    private int gridHeight;
    private int maxFloorCount;
    [Header("at the moment, MUST be 2")]
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
    private List<Vector2> freeTales;

    //padding
    private int heightPadding = 4;
    private int widthPadding = 7;

    // Simple class to track our miners
    private class Walker
    {
        public Vector2Int position;
        public Vector2Int direction;
    }

    private List<Walker> walkers;

    public void generateMap(int level)
    {
        setSpawnParameters(level); 
        SetupGrid();
        RunWalkers();
        SpawnGeometry();
    }

    private void setSpawnParameters(int level)
    {
        gridWidth = Mathf.RoundToInt(baseCaseGridWidth * Mathf.Pow(1f + widthIncreasePercentage, level - 1));
        gridHeight = Mathf.RoundToInt(baseCaseGridWidth * Mathf.Pow(1f + heightIncreasePercentage, level - 1));
        maxFloorCount = Mathf.RoundToInt((gridHeight-heightPadding)*(gridWidth-widthPadding)*floorPercentage);
    }

    void SetupGrid()
    {
        gridData = new int[gridWidth, gridHeight];
        freeTales = new List<Vector2>();

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

                // 2. Clamp position to grid bounds (leave some tiles padding for walls)
                walkers[i].position.x = Mathf.Clamp(walkers[i].position.x, widthPadding, gridWidth - widthPadding - 1);
                walkers[i].position.y = Mathf.Clamp(walkers[i].position.y, heightPadding, gridHeight -heightPadding - 1);

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

                    GameObject instance = Instantiate(floorPrefab[randomInt], pos, Quaternion.identity);
                    instance.transform.SetParent(levelManager.getBoardHolder().transform);
                }
                else // Wall
                {
                    // Optimization: Only spawn physics walls if they touch a floor
                    if (HasFloorNeighbor(x, y))
                    {
                        int r = Random.Range(0, physWallPrefab.Length);
                        GameObject instance = Instantiate(physWallPrefab[r], pos, Quaternion.identity);
                        instance.transform.SetParent(levelManager.getBoardHolder().transform);
                    }
                    else
                    {
                        int r = Random.Range(0, nonPhysWallPrefab.Length);
                        GameObject instance = Instantiate(nonPhysWallPrefab[r], pos, Quaternion.identity);
                        instance.transform.SetParent(levelManager.getBoardHolder().transform);
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

    private int getNormalizerForX()
    {
        return gridWidth / 2;
    }

    private int getNormalizerForY()
    {
        return gridHeight / 2;
    }

    private Vector2 getRandomFreeTale()
    {
        int randomInt = Random.Range(0, freeTales.Count);
        Vector2 randomFreeTale = freeTales[randomInt];
        freeTales.RemoveAt(randomInt);
        return randomFreeTale;
    }

    public Vector2 getRandomFreeTaleNormalized()
    {
        Vector2 pos = getRandomFreeTale();
        return new Vector2((pos.x - getNormalizerForX()) * tileSize, (pos.y - getNormalizerForY()) * tileSize);
    }
}
