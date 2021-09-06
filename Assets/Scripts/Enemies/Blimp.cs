using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blimp : MonoBehaviour, IExplodable, IEnemy
{
    [Header("Rocket Firing")]
    //[SerializeField] private GameObject rocketPrefab;
    //[SerializeField] private Transform rocketSpawnPoint;
    //[SerializeField] private float firingInterval;
    [SerializeField] private float maxFiringDistance;
    //private float timer = 0f;

    [Header("Mine Drop")]
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private Transform mineSpawnPoint;
    [SerializeField] private float layingInterval;
    private float layingTimer = 0f;

    [Header("Float Height Settings")]
    [SerializeField] private float heightTarget;
    [SerializeField] private float upwardFloatForce;
    [SerializeField] private float restingFloatForce;
    private ConstantForce floatForce;

    [Header("Self Explosion and Death")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionTimer;
    [SerializeField] private float numberOfBalloons;
    private bool dead = false;

    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private float rotationForce;
    [SerializeField] private float force;
    private Rigidbody rb;
    private float deltaTarget;

    // Start is called before the first frame update
    void Start()
    {
        floatForce = this.GetComponent<ConstantForce>();
        //timer = firingInterval;
        layingTimer = layingInterval;
        rb = this.GetComponent<Rigidbody>();
        if (target == null)
        {
            Transform tempCheck = GameObject.FindGameObjectWithTag("Player").transform;
            if (tempCheck == null)
            {
                Explode();
            }
            else
            {
                target = tempCheck;
            }
        }
        deltaTarget = Vector3.Distance
                (target.position, this.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (dead)
        {
            explosionTimer -= Time.deltaTime;
            if (explosionTimer <= 0)
            {
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
            //timer -= Time.deltaTime;
            layingTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            deltaTarget = Vector3.Distance
                   (target.position, this.transform.position);
            AdjustFloat();
            Turn();
            //If target is too far then move to him instead of firing
            if (deltaTarget> maxFiringDistance)
            {
                Move();
            }
            else {
                LayMines();
            }
            //Also resets firing timer
            //FireRockets();
        }
    }

    private void LayMines()
    {
        if (layingTimer <= 0)
        {
            layingTimer = layingInterval;
            Instantiate(minePrefab, mineSpawnPoint.position, mineSpawnPoint.rotation);
        }
    }

    //private void FireRockets()
    //{
    //    //Rocket Firing
    //    if (timer <= 0)
    //    {
    //        timer = firingInterval;
    //        Instantiate(rocketPrefab,
    //            rocketSpawnPoint.position, rocketSpawnPoint.rotation);
    //    }
    //}

    private void AdjustFloat()
    {
        //Float height target
        if (this.transform.position.y >= target.transform.position.y+15)
        {
            floatForce.force = new Vector3(0, restingFloatForce, 0);
        }
        else if(this.transform.position.y <= target.transform.position.y+5)
        {
            floatForce.force = new Vector3(0, upwardFloatForce, 0);
        }
    }

    private void Turn()
    {
        Vector3 direction = target.position - rb.position; //getting direction
        direction.Normalize(); //erasing magnitude
        Vector3 rotationAmount = Vector3.Cross(transform.forward, direction);//calculating angle
        //rb.angularVelocity = rotationAmount * rotationForce;
        rb.AddRelativeTorque(rotationAmount * rotationForce);
        //Debug.Log(rotationAmount +"->"+rb.angularVelocity);

        rb.AddRelativeTorque(rotationAmount * rotationForce);
    }

    private void Move()
    {
        //rb.velocity = transform.forward * force;
        rb.AddForce(transform.forward * force);
        //Debug.Log(transform.forward+"->"+rb.velocity);
    }

    public void Explode()
    {
        //instantiate effect
        Instantiate(explosionPrefab, transform.position, transform.rotation);

        //apply force to nearby rigidbodies with colliders
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(
                    explosionForce, transform.position, explosionRadius);
            }
        }
        Destroy(this.gameObject);
    }

    public void ReduceBalloonCount()
    {
        numberOfBalloons -= 1;
        if (numberOfBalloons <= 0)
        {
            floatForce.force = new Vector3(0, 0, 0);
            dead = true;
        }
    }
}
