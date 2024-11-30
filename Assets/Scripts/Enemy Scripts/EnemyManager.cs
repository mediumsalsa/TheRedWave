using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject slimePrefab;
    public GameObject cocogruntPrefab;
    public Transform[] spawnPoints;

    public void SpawnEnemy(string enemyType)
    {
        GameObject enemy = null;
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (enemyType == "Slime")
            enemy = Instantiate(slimePrefab, spawnPoint.position, Quaternion.identity);
        else if (enemyType == "Cocogrunt")
            enemy = Instantiate(cocogruntPrefab, spawnPoint.position, Quaternion.identity);

        if (enemy != null)
            Debug.Log($"{enemyType} spawned!");
    }
}
