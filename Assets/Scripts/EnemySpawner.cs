using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval;
    private float spawnTimer;
    [SerializeField] private Transform spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            int spawnChoice = Random.Range(0, enemyPrefabs.Length);
            spawnTimer = spawnInterval + (11*spawnChoice);
            Vector3 originPoint = spawnPoint.position;
            Vector3 randomRange = new Vector3(Random.Range(-20, 20),
                                  Random.Range(-10, 10),
                                  Random.Range(-20, 20));
            Vector3 randomCoordinate = new Vector3();
            randomCoordinate = originPoint + randomRange;
            Instantiate(enemyPrefabs[spawnChoice], randomCoordinate, spawnPoint.rotation);
        }
    }
}
