using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject slimePrefab;
    public GameObject cocogruntPrefab;

    [Header("Spawn Settings")]
    public Transform[] topSpawnPoints; // Top strip spawn points
    public Transform[] bottomSpawnPoints; // Bottom strip spawn points

    [Header("Wave Settings")]
    public int initialSlimeCount = 4; // Number of slimes in wave 1
    public float spawnDelay = 0.2f; // Delay between spawns
    public Transform player; // Player's transform
    public Vector3 playerResetPosition = Vector3.zero; // Middle of the map

    private int currentWave = 1;
    private int enemiesToSpawn;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        StartWave();
    }

    private void Update()
    {
        // Check if all enemies are defeated
        activeEnemies.RemoveAll(enemy => enemy == null); // Clean up destroyed enemies
        if (activeEnemies.Count == 0)
        {
            NextWave();
        }
    }

    private void StartWave()
    {
        Debug.Log($"Wave {currentWave} starting!");
        enemiesToSpawn = initialSlimeCount + (currentWave - 1) * 2; // Increase enemy count each wave
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        for (int i = 0; i < enemiesToSpawn / 2; i++)
        {
            // Spawn on top strip
            SpawnEnemyAtRandomPoint(slimePrefab, topSpawnPoints);
            enemiesToSpawn--; // Decrement after spawning
            yield return new WaitForSeconds(spawnDelay);

            // Spawn on bottom strip
            SpawnEnemyAtRandomPoint(cocogruntPrefab, bottomSpawnPoints);
            enemiesToSpawn--; // Decrement after spawning
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnEnemyAtRandomPoint(GameObject enemyPrefab, Transform[] spawnPoints)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    private void NextWave()
    {
        currentWave++;
        ResetPlayerPosition();
        StartWave();
    }

    private void ResetPlayerPosition()
    {
        if (player != null)
        {
            player.position = playerResetPosition;
            Debug.Log("Player reset to the middle of the map.");
        }
    }
}
