using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pollen_Child : MonoBehaviour, IExplodable, IEnemy
{
    [Header("Laser Firing")]
    [SerializeField] private GameObject laserCollider;
    [SerializeField] private Transform laserFirePoint;
    [SerializeField] private LineRenderer laserRenderer;
    [SerializeField] private float laserInterval;
    [SerializeField] private float laserDuration;
    [SerializeField] private float maxFiringDistance;
    [SerializeField] private AudioSource laserAudio;
    private float laserITimer = 0f, laserDTimer=0f;
    private bool isFiringLaser = false;

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

    private bool active = false;
    private float startDelay = 3f;
    [SerializeField] private Animator balloonAnimator;

    // Start is called before the first frame update
    void Start()
    {
        floatForce = this.GetComponent<ConstantForce>();
        laserITimer = laserInterval;
        laserDTimer = laserDuration;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
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

                if (!isFiringLaser)
                {
                    laserITimer -= Time.deltaTime;
                }
                else
                {
                    laserDTimer -= Time.deltaTime;
                }
            }
        }
        else {
            startDelay -= Time.deltaTime;
            if (startDelay <= 0) {
                Activate();
            }
        }
       
    }

    private void FixedUpdate()
    {
        if (!dead && active)
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
            ToggleLaser();
        }
    }

    private void Activate() {
        active = true;
        balloonAnimator.SetBool("deflated",false);
        GetComponent<Balloon>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;
        GetComponent<ConstantForce>().enabled = true;
    }

    private void ToggleLaser()
    {

        if (isFiringLaser)
        {
            laserRenderer.enabled = true;
            int layer = 1 << LayerMask.NameToLayer("Default");
            RaycastHit hit;
            if (Physics.Raycast(laserFirePoint.transform.position,
                laserFirePoint.transform.TransformDirection(Vector3.forward), out hit, maxFiringDistance, layer, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                laserRenderer.SetPosition(1, new Vector3(0, 0, hit.distance));
                laserCollider.transform.localScale = new Vector3(3.5f, 3.5f, hit.distance);
                laserCollider.transform.localPosition = new Vector3(0, 0, hit.distance / 2);
            }
            else
            {
                laserRenderer.SetPosition(1, new Vector3(0, 0, maxFiringDistance));
                laserCollider.transform.localScale = new Vector3(3.5f, 3.5f, maxFiringDistance);
                laserCollider.transform.localPosition = new Vector3(0, 0, maxFiringDistance/2+3);
            }
            laserAudio.volume = SettingsMenu.sfxVolume;
            laserAudio.UnPause();
            if (!laserAudio.isPlaying) {
                laserAudio.Play();
            }

            if (laserDTimer <= 0)
            {
                //Turn off laser 
                laserDTimer = laserDuration;
                isFiringLaser = false;
            }
        }
        else {
            laserRenderer.enabled = false;
            laserCollider.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            laserCollider.transform.localPosition = new Vector3(0, 0, 2.5f);
            laserAudio.Pause();
            if (laserITimer <= 0) {
                //Turn on laser 
                laserITimer = laserInterval;
                isFiringLaser = true;
            }
        }
    }

    private void AdjustFloat()
    {
        //Float height target
        if (this.transform.position.y > target.position.y+1)
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
        Vector3 rotateToPlayer = Vector3.Cross(transform.forward, direction);//calculating angle
        rotateToPlayer = Vector3.Project(rotateToPlayer, transform.up);
        rb.AddRelativeTorque(rotateToPlayer * rotationForce);
        //Debug.Log(rotationAmount +"->"+rb.angularVelocity);

        //Local up to World Up
        Vector3 predictedUp = Quaternion.AngleAxis(
             rb.angularVelocity.magnitude * Mathf.Rad2Deg * rotationForce / force,
             rb.angularVelocity
         ) * transform.up;
        Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(torqueVector * force * 5);
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
            balloonAnimator.SetBool("deflated", true);
            floatForce.force = new Vector3(0, 0, 0);
            dead = true;
        }
    }
}
