using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pollen_Spine : MonoBehaviour, IExplodable, IEnemy
{
    [Header("Rocket Firing")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private Transform[] rocketSpawnPoints;
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
    [SerializeField] private Transform[] pollenChildren;
    private float deltaTarget;
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
        //rb.inertiaTensor = new Vector3(1,1,1);
        rb.inertiaTensorRotation = Quaternion.identity;
        rb.centerOfMass = new Vector3(0, 0, 0);
        deltaTarget = Vector3.Distance
                (target.position, this.transform.position);
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
            deltaTarget = Vector3.Distance
                    (target.position, this.transform.position);
            AdjustFloat();
            //Turn();
            //If target is too far then move to him instead of firing
            if (deltaTarget > maxFiringDistance)
            {
                Move();
            }
            //Also resets firing timer
            FireRockets();
        }
    }

    private void FireRockets() {
        //Rocket Firing
        if (timer <= 0)
        {
            timer = firingInterval;
            foreach (Transform point in rocketSpawnPoints)
            {
                Instantiate(rocketPrefab,
                    point.position, point.rotation);
            }
        }
    }

    private void AdjustFloat() {
        //Float height target
        if (this.transform.position.y > target.transform.position.y)
        {
            floatForce.force = new Vector3(0, restingFloatForce, 0);
        }
        else
        {
            floatForce.force = new Vector3(0, upwardFloatForce, 0);
        }
    }

    private void Turn() {
        //Forward unto player
        Vector3 direction = target.position - transform.position; //getting direction
        direction.Normalize(); //erasing magnitude
        Vector3 rotationAmount = Vector3.Cross(transform.forward, direction);//calculating angle
        //rb.AddRelativeTorque(rotationAmount * 3);
        //Debug.Log(rotationAmount +"-"+Vector3.Angle(transform.forward, direction));
        //Debug.DrawLine(transform.position, rotationAmount, Color.green, 1f);

        Quaternion toTarget = Quaternion.FromToRotation(transform.forward,direction);
        Quaternion uprightBalance = Quaternion.FromToRotation(transform.up, Vector3.up);
        uprightBalance *= toTarget;
        Vector3 rotate = new Vector3(uprightBalance.x, uprightBalance.y, uprightBalance.z);
        rb.AddTorque(rotate * rotationForce);
        //Debug.Log(toTarget+"X"+rotate+"="+uprightBalance);


        //   Vector3 predictedUp = Quaternion.AngleAxis(
        //    rb.angularVelocity.magnitude * Mathf.Rad2Deg * rotationForce / force,
        //    rb.angularVelocity
        //) * transform.up;
        //   Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
        //   rb.AddTorque(torqueVector * force * force);

        //Local up to World Up
        //Vector3 yCorrection = Vector3.Cross(transform.up, Vector3.up);
        //yCorrection.Normalize();
        //rb.AddRelativeTorque(yCorrection * (rotationForce));
        //Debug.Log(yCorrection + "--" + Vector3.Angle(transform.up, Vector3.up));
        ////Do both?
        //Vector3 upAndAway = Vector3.Cross(yCorrection, rotationAmount);
        //upAndAway.Normalize();
        //rb.AddRelativeTorque(upAndAway * rotationForce);
        //Debug.DrawLine(transform.position, upAndAway, Color.green, 1f);
        //Debug.Log(rotationAmount+"X"+yCorrection+"="+upAndAway);

    }

    private void Move() {
        rb.AddForce(transform.forward * force);
        Debug.Log(rb.velocity+"--"+rb.velocity.magnitude);
        Debug.DrawLine(transform.position,transform.forward, Color.green,1f);
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
            foreach (Transform child in pollenChildren)
            {
                child.GetComponent<Pollen_Child>().enabled = true;
                child.SetParent(null);
            }
        }
    }
}
