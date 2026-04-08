using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    private class EnemyProfile
    {
        public GameObject prefab;
        public float strength;
        public EnemyProfile(float strength, GameObject prefeab)
        {
            this.strength = strength;
            this.prefab = prefeab;
        }
    }

    public LevelManager levelManager;
    public MapManager mapManager;

    [Header("Standard Enemies (MUST be ordered by strength ascending)")]
    [Tooltip("Include basicEnemy, nonBasicEnemy, shotgunEnemy, sniperEnemy, arEnemy")]
    public List<GameObject> standardEnemiesAsGameObjects;
    private List<EnemyProfile> standardEnemies;

    [Header("Boss Enemy")]
    [Tooltip("The bossEnemy profile")]
    public GameObject bossEnemyAsGameObject;
    private EnemyProfile bossEnemy;

    [Header("Spawn Parameters")]
    [Tooltip("Specific 3: The base amount of enemies for Level 1")]
    public int baseEnemyCountLevel1 = 15;

    [Tooltip("Specific 1: Percentage increase in total enemies per level (e.g., 0.2 = +20%)")]
    public float countIncreasePercentage = 0.20f;

    [Tooltip("Specific 1: Percentage increase in average strength per level (e.g., 0.15 = +15%)")]
    public float strengthIncreasePercentage = 0.50f;

    // Specific 4: Base average strength is NOT parametric (hardcoded constant).
    private float BaseAverageStrength;

    [Header("Allows to randomly spawn an enemy; regardless of needed strenthAvg")]
    public float chanceToSpawnRandomEnemy;

    [Header("Strength Parameters, use these parameter to balance the spawn")]
    public float basicEnemyStrenth = 1f;
    public float nonBasicEnemyStrenth = 2f;
    public float shotgunEnemyStrenth= 3f;
    public float sniperEnemyStrenth = 4f;
    public float arEnemyStrenth = 5f;
    public float bossEnemyStrenth = 8f;

    private void prepareForExecution()
    {
        if (standardEnemies != null && bossEnemy != null) { return; }
        //create the EnemyProfiles for common enemies
        List<float> strenghts = new List<float> { basicEnemyStrenth, nonBasicEnemyStrenth, shotgunEnemyStrenth, sniperEnemyStrenth,
        arEnemyStrenth};
        standardEnemies = new List<EnemyProfile>();
        for(int i = 0; i<5; i++)
            standardEnemies.Insert(i, new EnemyProfile(strenghts[i], standardEnemiesAsGameObjects[i]));
        //EnemyProfile for the boss
        bossEnemy = new EnemyProfile(bossEnemyStrenth, bossEnemyAsGameObject);
        //initialize BaseAverageStrength, THIS LINE SETS THE BASE FOR STRENGTH AVG
        BaseAverageStrength = 0.75f * basicEnemyStrenth + 0.25f * nonBasicEnemyStrenth;
    }

    /// <summary>
    /// Generates and instantiates the enemies for the given level.
    /// </summary>
    public void SpawnEnemiesForLevel(int level)
    {        
        prepareForExecution();  
        // 1. Calculate Target Count & Strength based on level scaling
        int targetEnemyCount = Mathf.RoundToInt(baseEnemyCountLevel1 * Mathf.Pow(1f + countIncreasePercentage, level - 1));
        levelManager.setNenemies(targetEnemyCount);
        float targetAvgStrength = BaseAverageStrength * Mathf.Pow(1f + strengthIncreasePercentage, level - 1);

        // Total "strength budget" we want to spend on this level
        float targetTotalStrength = targetEnemyCount * targetAvgStrength;

        List<EnemyProfile> enemiesToSpawn = new List<EnemyProfile>();

        // 2. Handle the Boss (Specific 2: if and only if level is a multiple of 3)
        if (level % 3 == 0)
        {
            enemiesToSpawn.Add(bossEnemy);
            levelManager.setNBosses(1); //per il momento ok, ma sarebbe da cambiare con level / 3: ex se siamo al livello 9 spawno 3 boss
            targetEnemyCount--;
            targetTotalStrength -= bossEnemy.strength;

            // Safety clamp: The boss must spawn but it may consume too much strength -> no  strength enough to spawn needed enemies.
            // If it is so, set targetTotalStrength so that it is possible to spawn other enemies.
            float minRequiredStrength = targetEnemyCount * standardEnemies[0].strength;
            targetTotalStrength = Mathf.Max(targetTotalStrength, minRequiredStrength);
        }

        // 3. Populate standard enemies using mathematical approximation (Specific 5)
        int remainingCount = targetEnemyCount;
        float remainingStrength = targetTotalStrength;

        for (int i = 0; i < targetEnemyCount; i++)
        {
            // To improve diversification, there is a chanche to return a random enemy, regardless of the avg strength needed
            if (Random.value <= chanceToSpawnRandomEnemy)
            {
                int randomIndexNormalized = (int)(Random.value * 100 % standardEnemies.Count);
                enemiesToSpawn.Add(standardEnemies[randomIndexNormalized]);
                remainingCount--;
                // Safety clamp: Choosing randomly may consume too much strength -> no  strength enough to spawn needed enemies.
                // If it is so, set remainingStrength so that it is possible to spawn other enemies.
                float minRequiredStrength = targetEnemyCount * standardEnemies[0].strength;
                remainingStrength = Mathf.Max(remainingStrength, minRequiredStrength);
                continue;
            }
            float neededAvg = remainingStrength / remainingCount;
            EnemyProfile selectedEnemy = SelectStandardEnemy(neededAvg);

            enemiesToSpawn.Add(selectedEnemy);
            remainingCount--;
            remainingStrength -= selectedEnemy.strength;
        }

        // 4. Instantiate the calculated pool
        foreach (var enemy in enemiesToSpawn)
        {
            InstantiateEnemy(enemy.prefab);
        }
    }

    /// <summary>
    /// Probabilistically selects an enemy type to tightly adhere to the needed average strength.
    /// </summary>
    private EnemyProfile SelectStandardEnemy(float neededAvg)
    {
        // If the needed average is lower than our weakest enemy, spawn the weakest
        if (neededAvg <= standardEnemies[0].strength)
            return standardEnemies[0];

        // If the needed average is higher than our strongest standard enemy, spawn the strongest
        if (neededAvg >= standardEnemies[standardEnemies.Count - 1].strength)
            return standardEnemies[standardEnemies.Count - 1];

        // Find the bracket (e.g., if neededAvg is 3.5, find enemies with strength 2.0 and 4.0)
        for (int i = 0; i < standardEnemies.Count - 1; i++)
        {
            EnemyProfile lower = standardEnemies[i];
            EnemyProfile upper = standardEnemies[i + 1];

            if (neededAvg >= lower.strength && neededAvg <= upper.strength)
            {
                // Calculate the probability of picking the stronger enemy in the bracket
                float strengthRange = upper.strength - lower.strength;
                float chanceForUpper = (neededAvg - lower.strength) / strengthRange;

                // Roll a random float between 0.0 and 1.0
                if (Random.value <= chanceForUpper)
                    return upper;
                else
                    return lower;
            }
        }

        // Fallback
        return standardEnemies[0];
    }

    private void InstantiateEnemy(GameObject prefab)
    {
        if (prefab == null) return;
        Vector2 spawnPosition = mapManager.getRandomFreeTaleNormalized();
        GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
        instance.GetComponent<EnemyLifeCycle>().levelManager = levelManager;
        instance.transform.SetParent(levelManager.getBoardHolder().transform);
    }
}