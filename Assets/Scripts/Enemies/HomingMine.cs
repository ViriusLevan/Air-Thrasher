using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMine : MonoBehaviour, IExplodable, IEnemy
{
    [Header("Float Height Settings")]
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

    [SerializeField] private Animator balloonAnimator;
    [SerializeField] private SphereCollider sCollider;
    [SerializeField] private CapsuleCollider cCollider;
    [SerializeField] private float timeBeforeActivation;
    private bool active = false;


    // Start is called before the first frame update
    void Start()
    {
        floatForce = this.GetComponent<ConstantForce>();
        rb = this.GetComponent<Rigidbody>();
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Time before afterdeath explosion 
        if (dead)
        {
            explosionTimer -= Time.deltaTime;
            if (explosionTimer <= 0)
            {
                Explode();
            }
        }
        //If no target then explode
        if (target == null)
        {
            Explode();
        }
        timeBeforeActivation -= Time.deltaTime;
        if (timeBeforeActivation <= 0) {
            Activate();
        }
    }

    private void FixedUpdate()
    {
        if (active && !dead)
        {
            AdjustFloat();
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<Player>().EnemyMineHit(1);
            Explode();
        }
    }

    private void Activate() {
        active = true;
        balloonAnimator.enabled = true;
        sCollider.enabled = true;
        cCollider.enabled = true;
        floatForce.enabled = true;
    }

    private void AdjustFloat()
    {
        //Float height target
        if (this.transform.position.y >= 300)
        {
            floatForce.force = new Vector3(0, 9, 0);
        }
        else
        {
            floatForce.force = new Vector3(0, 11, 0);
        }

        //Local up to World Up
        Vector3 predictedUp = Quaternion.AngleAxis(
             rb.angularVelocity.magnitude * Mathf.Rad2Deg * rotationForce / force,
             rb.angularVelocity
         ) * transform.up;
        Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(torqueVector * force * force);
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