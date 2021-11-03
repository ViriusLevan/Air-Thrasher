using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static int enemyNumber=5;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval;
    [SerializeField] private float spawnMultiplier;
    private float spawnTimer;
    private float timer;
    [SerializeField] private Transform spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = spawnInterval;
        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0 && enemyNumber <60)
        {
            enemyNumber += 1;
            int spawnChoice = Random.Range(0, enemyPrefabs.Length);
            spawnTimer = spawnInterval + (spawnMultiplier*spawnChoice);
            Vector3 originPoint = spawnPoint.position;
            Vector3 randomRange = new Vector3(Random.Range(-20, 20),
                                  Random.Range(-10, 10),
                                  Random.Range(-20, 20));
            Vector3 randomCoordinate = new Vector3();
            randomCoordinate = originPoint + randomRange;
            Instantiate(enemyPrefabs[spawnChoice], randomCoordinate, spawnPoint.rotation);
        }

        if (spawnMultiplier > 0)
            timer += Time.deltaTime;
        if (timer > 20) {
            timer = 0;
            spawnMultiplier -= 1;
        }
    }
}
