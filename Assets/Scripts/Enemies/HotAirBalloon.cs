using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotAirBalloon : BalloonEnemy
{
    [Header("Rocket Firing")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private Transform rocketSpawnPoint;
    [SerializeField] private float firingInterval;
    [SerializeField] private float maxFiringDistance;
    private float timer=0f;

    private float stuckTimer, stuckDuration = 5f;
    private GameObject stuckWith;
 
    // Start is called before the first frame update
    void Start()
    {
        floatForce = this.GetComponent<ConstantForce>();
        timer = firingInterval;
        rb = this.GetComponent<Rigidbody>();
        if (target == null) {
            Transform tempCheck = GameObject.FindGameObjectWithTag("Player").transform;
            if (tempCheck == null)
            {
                InvokeDeathEvent(BalloonEnemyType.HotAirBalloon);
                Explode();
            }
            else {
                target = tempCheck;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;
        if (dead)
        {
            explosionTimer -= Time.deltaTime;
            if (explosionTimer <= 0) {
                Explode();
            }
        }
        else
        {
            //If no target then explode
            if (target == null) {
                Explode();
            }

            //Timer for Rocket Firing continued in FixedUpdate
            timer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            AdjustFloat();
            Turn();
            //If target is too far then move to him instead of firing
            if (Vector3.Distance
                (target.position, this.transform.position)
                > maxFiringDistance)
            {
                Move();
            }
            //Also resets firing timer
            FireRockets();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        stuckTimer -= Time.deltaTime;
        if (stuckWith != collision.gameObject) { 
            stuckTimer = stuckDuration;
            stuckWith = collision.gameObject;
        }
        if (stuckTimer <= 0) {
            // Calculate Angle Between the collision point and this
            Vector3 dir = collision.contacts[0].point - transform.position;
            // We then get the opposite (-Vector3) and normalize it
            dir = -dir.normalized;
            // And finally we add force in the direction of dir and multiply it by force. 
            // This will push back the player
            rb.AddForce(dir * force);
        }
    }

    private void FireRockets() {
        //Rocket Firing
        if (timer <= 0)
        {
            timer = firingInterval;
            Instantiate(rocketPrefab, 
                rocketSpawnPoint.position, rocketSpawnPoint.rotation);
        }
    }
}
