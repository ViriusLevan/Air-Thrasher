using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powell : BalloonEnemy
{
    [Header("Rocket Firing")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private Transform[] rocketSpawnPoints;
    [SerializeField] private float firingInterval;
    [SerializeField] private float maxFiringDistance;
    private float timer = 0f;

    [Header("Mine Drop")]
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private Transform mineSpawnPoint;
    [SerializeField] private float layingInterval;
    private float layingTimer = 0f;

    private float deltaTarget;

    // Start is called before the first frame update
    void Start()
    {
        floatForce = this.GetComponent<ConstantForce>();
        timer = firingInterval;
        layingTimer = layingInterval;
        rb = this.GetComponent<Rigidbody>();
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        deltaTarget = Vector3.Distance
                (target.position, this.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;
        if (dead)
        {
            explosionTimer -= Time.deltaTime;
            if (explosionTimer <= 0)
            {
                InvokeDeathEvent(BalloonEnemyType.Powell);
                Explode();
            }
        }
        else
        {
            //If no target then explode
            if (target == null)
            {
                Explode();
            }
            //Timer for Rocket Firing continued in FixedUpdate
            timer -= Time.deltaTime;
            layingTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            AdjustFloat();
            Turn();
            deltaTarget = Vector3.Distance
                    (target.position, this.transform.position);
            //If target is too far then move to him instead of firing
            Move();
            if (deltaTarget <= maxFiringDistance)
            {
                //Also resets firing timer
                FireRockets();
            }
            //Also resets laying timer
            LayMines();
        }
    }

    private void LayMines() {
        if (layingTimer <= 0)
        {
            layingTimer = layingInterval;
            Instantiate(minePrefab, mineSpawnPoint.position, mineSpawnPoint.rotation);
        }
    }

    private void FireRockets()
    {
        if (timer <= 0)
        {
            timer = firingInterval;
            foreach (Transform sPoint in rocketSpawnPoints)
            {
                Instantiate(rocketPrefab, sPoint.position, sPoint.rotation);
            }
        }
    }

    new protected void AdjustFloat()
    {
        //Float height target
        if (this.transform.position.y-5>= target.position.y)
        {
            floatForce.force = new Vector3(0, restingFloatForce, 0);
        }
        else
        {
            floatForce.force = new Vector3(0, upwardFloatForce, 0);
        }
    }
}
