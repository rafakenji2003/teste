using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] waypoints;
    public int enemiesPerWave = 5;
    public float timeBetweenEnemies = 0.8f;
    public float timeBetweenWaves = 5f;

    private int currentWave = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            currentWave++;
            yield return StartCoroutine(SpawnWave());
            yield return new WaitForSeconds(timeBetweenWaves);
            enemiesPerWave += 2; // aumenta dificuldade
        }
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenEnemies);
        }
    }

    void SpawnEnemy()
    {
        GameObject obj = Instantiate(enemyPrefab, waypoints[0].position, Quaternion.identity);
        obj.GetComponent<Enemy>().SetWaypoints(waypoints);
    }
}
