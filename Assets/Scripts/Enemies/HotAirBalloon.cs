using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotAirBalloon : MonoBehaviour, IExplodable, IEnemy
{
    [Header("Rocket Firing")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private Transform rocketSpawnPoint;
    [SerializeField] private float firingInterval;
    [SerializeField] private float maxFiringDistance;
    private float timer=0f;

    [Header("Float Height Settings")]
    [SerializeField] private float upwardFloatForce;
    [SerializeField] private float restingFloatForce;
    private ConstantForce floatForce;

    [Header("Self Explosion and Death")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionTimer;
    [SerializeField] private float numberOfBalloons;
    private bool dead=false;
    
    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private float rotationForce;
    [SerializeField] private float force;
    private Rigidbody rb;

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

    private void AdjustFloat() {
        //Float height target
        if (this.transform.position.y >= target.position.y)
        {
            floatForce.force = new Vector3(0, restingFloatForce, 0);
        }
        else
        {
            floatForce.force = new Vector3(0, upwardFloatForce, 0);
        }
    }

    private void Turn() {
        Vector3 direction = target.position - rb.position; //getting direction
        direction.Normalize(); //erasing magnitude
        Vector3 rotationAmount = Vector3.Cross(transform.forward, direction);//calculating angle
        rb.AddRelativeTorque(rotationAmount * rotationForce);
        //Debug.Log(rotationAmount +"->"+rb.angularVelocity);

        //Local up to World Up
        Vector3 predictedUp = Quaternion.AngleAxis(
         rb.angularVelocity.magnitude * Mathf.Rad2Deg * rotationForce / force,
         rb.angularVelocity
     ) * transform.up;
        Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(torqueVector * force * force);
    }

    private void Move() {
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
        if (numberOfBalloons <= 0) {
            floatForce.force = new Vector3(0, 0, 0);
            dead = true;
        }
    }

}
