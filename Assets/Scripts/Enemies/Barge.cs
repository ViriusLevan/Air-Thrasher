using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barge : MonoBehaviour, IExplodable, IEnemy
{
    [Header("Cannon Firing")]
    [SerializeField] private GameObject bargeBullet;
    [SerializeField] private Transform[] cannonPointsLeft;
    [SerializeField] private Transform[] cannonPointsRight;
    [SerializeField] private float firingInterval;
    [SerializeField] private float maxFiringDistance;
    private float timer = 0f;

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
    private bool dead = false;

    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private float rotationForce;
    [SerializeField] private float force;
    private Rigidbody rb;
    private float deltaTarget;
    private float deltaLeftTarget, deltaRightTarget;

    // Start is called before the first frame update
    void Start()
    {
        floatForce = this.GetComponent<ConstantForce>();
        timer = firingInterval;
        rb = this.GetComponent<Rigidbody>();
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        deltaTarget = Vector3.Distance
                (target.position, this.transform.position);
        deltaRightTarget = Vector3.Angle(transform.right, target.position);
        deltaLeftTarget = Vector3.Angle(transform.right*-1, target.position);
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
            timer -= Time.deltaTime;
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
            deltaRightTarget = Vector3.Angle(transform.right, target.position);
            deltaLeftTarget = Vector3.Angle(transform.right * -1, target.position);
            //If target is too far then move to him instead of firing
            if (deltaTarget > maxFiringDistance)
            {
                Move();
            }
            else
            {
                FireCannons();
            }
        }
    }

    private void FireCannons()
    {
        if (timer <= 0)
        {
            if (deltaRightTarget < deltaLeftTarget)
            {
                timer = firingInterval;
                foreach (Transform sPoint in cannonPointsRight)
                {
                    Instantiate(bargeBullet, sPoint.position, sPoint.rotation);
                }
            }
            else
            {
                timer = firingInterval;
                foreach (Transform sPoint in cannonPointsLeft)
                {
                    Instantiate(bargeBullet, sPoint.position, sPoint.rotation);
                }
            }
        }
    }

    private void AdjustFloat()
    {
        //Float height target
        if (this.transform.position.y > target.transform.position.y + 1)
        {
            floatForce.force = new Vector3(0, restingFloatForce, 0);
        }
        else
        {
            floatForce.force = new Vector3(0, upwardFloatForce, 0);
        }
    }

    private void Turn()
    {
        Vector3 direction = target.position - rb.position; //getting direction
        direction.Normalize(); //erasing magnitude
        Vector3 rotationAmount = new Vector3();
        if (deltaTarget <= maxFiringDistance)
        {
            if (deltaRightTarget < deltaLeftTarget)
            {
                rotationAmount = Vector3.Cross(transform.right, direction);//calculating angle
            }
            else {
                rotationAmount = Vector3.Cross(transform.right*-1, direction);//calculating angle
            }
        }
        else
        {
            rotationAmount = Vector3.Cross(transform.forward, direction);//calculating angle
        }
        rb.AddRelativeTorque(rotationAmount * rotationForce);
        //Debug.Log(rotationAmount +"->"+rb.angularVelocity);
    }

    private void Move()
    {
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
