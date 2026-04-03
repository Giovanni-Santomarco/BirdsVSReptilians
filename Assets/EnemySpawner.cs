using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public struct EnemyProfile
    {
        public string name;
        public GameObject prefab;
        public float strength;
    }

    [Header("Standard Enemies (MUST be ordered by strength ascending)")]
    [Tooltip("Include basicEnemy, nonBasicEnemy, shotgunEnemy, sniperEnemy, arEnemy")]
    public List<EnemyProfile> standardEnemies;

    [Header("Boss Enemy")]
    [Tooltip("The bossEnemy profile")]
    public EnemyProfile bossEnemy;

    [Header("Spawn Parameters")]
    [Tooltip("Specific 3: The base amount of enemies for Level 1")]
    public int baseEnemyCountLevel1 = 15;

    [Tooltip("Specific 1: Percentage increase in total enemies per level (e.g., 0.2 = +20%)")]
    public float countIncreasePercentage = 0.20f;

    [Tooltip("Specific 1: Percentage increase in average strength per level (e.g., 0.15 = +15%)")]
    public float strengthIncreasePercentage = 0.15f;

    // Specific 4: Base average strength is NOT parametric (hardcoded constant).
    // Assuming basicEnemy strength is ~1.0 and nonBasic is ~2.0, 
    // an average of 1.25 guarantees mostly basicEnemies and a few nonBasicEnemies.
    private const float BaseAverageStrength = 1.25f;


    [Header("Strength Parameters, use these parameter to balance the spawn")]
    public float basicEnemyStrenth;
    public float nonBasicEnemyStrenth;
    public float shotgunEnemyStrenth;
    public float sniperEnemyStrenth;
    public float arEnemyStrenth;
    public float bossEnemyStrenth;

    /// <summary>
    /// Generates and instantiates the enemies for the given level.
    /// </summary>
    public void SpawnEnemiesForLevel(int level)
    {
        // 1. Calculate Target Count & Strength based on level scaling
        int targetEnemyCount = Mathf.RoundToInt(baseEnemyCountLevel1 * Mathf.Pow(1f + countIncreasePercentage, level - 1));
        float targetAvgStrength = BaseAverageStrength * Mathf.Pow(1f + strengthIncreasePercentage, level - 1);

        // Total "strength budget" we want to spend on this level
        float targetTotalStrength = targetEnemyCount * targetAvgStrength;

        List<EnemyProfile> enemiesToSpawn = new List<EnemyProfile>();

        // 2. Handle the Boss (Specific 2: if and only if level is a multiple of 3)
        if (level % 3 == 0)
        {
            enemiesToSpawn.Add(bossEnemy);
            targetEnemyCount--;
            targetTotalStrength -= bossEnemy.strength;

            // Safety clamp: Prevent the boss from consuming so much strength budget 
            // that the remaining enemies require negative strength.
            float minRequiredStrength = targetEnemyCount * standardEnemies[0].strength;
            targetTotalStrength = Mathf.Max(targetTotalStrength, minRequiredStrength);
        }

        // 3. Populate standard enemies using mathematical approximation (Specific 5)
        int remainingCount = targetEnemyCount;
        float remainingStrength = targetTotalStrength;

        for (int i = 0; i < targetEnemyCount; i++)
        {
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

        // Replace with your game's actual spawn-point logic (e.g., finding a valid floor tile)
        Vector2 spawnPosition = Random.insideUnitCircle * 10f;
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}