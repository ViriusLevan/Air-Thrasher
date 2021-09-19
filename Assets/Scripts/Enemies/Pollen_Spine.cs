using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pollen_Spine : MonoBehaviour, IExplodable, IEnemy
{
    [Header("Laser Firing")]
    [SerializeField] private GameObject[] laserColliders;
    [SerializeField] private Transform[] laserFirePoints;
    [SerializeField] private LineRenderer[] laserRenderers;
    [SerializeField] private float laserInterval;
    [SerializeField] private float laserDuration;
    [SerializeField] private AudioSource[] laserAudio;
    private float laserITimer = 0f, laserDTimer = 0f;
    private bool isFiringLaser = false;

    [Header("Rocket Firing")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private Transform[] rocketSpawnPoints;
    [SerializeField] private float firingInterval;
    [SerializeField] private float maxFiringDistance;
    private float timer=0f;

    [Header("Float Height Settings")]
    [SerializeField] private float upwardFloatForce;
    [SerializeField] private float restingFloatForce;
    [SerializeField] private float heightDifference;
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
    private float deltaTarget, stuckTimer, stuckInterval = 5f;
    private bool reverseCourse=false;
    private Collision stuckCollision;

    public delegate void OnPollenSpineDeath();
    public static event OnPollenSpineDeath spineDeath;

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
        //rb.inertiaTensorRotation = Quaternion.identity;
        //rb.centerOfMass -= new Vector3(-2.5f, -2.5f, 0);
        deltaTarget = Vector3.Distance
                (target.position, this.transform.position);
        stuckTimer = stuckInterval;
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

            if (!isFiringLaser)
            {
                laserITimer -= Time.deltaTime;
            }
            else
            {
                laserDTimer -= Time.deltaTime;
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
            RotateAxis();
            //If target is too far then move to him instead of firing
            if (deltaTarget > maxFiringDistance)
            {
                Move();
            }
            //Also resets firing timer
            FireRockets();
            ToggleLaser();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        stuckCollision = collision;
        if (collision == stuckCollision)
        {
            stuckTimer -= Time.deltaTime;
            if (stuckTimer <= 0)
            {
                stuckTimer = stuckInterval;
                reverseCourse = !reverseCourse;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (stuckCollision == collision && reverseCourse)
        {
            StartCoroutine(ReverseCountdown());
        }
        else
        {
            stuckTimer = stuckInterval;
        }
    }

    private void ToggleLaser()
    {

        if (isFiringLaser)
        {
            foreach (LineRenderer lr in laserRenderers)
            {
                lr.enabled = true;
            }
            int layer = 1 << LayerMask.NameToLayer("Default");
            
            RaycastHit hit; 
            for (int i = 0; i < laserFirePoints.Length; i++)
            {
                if (Physics.Raycast(laserFirePoints[i].transform.position,
                laserFirePoints[i].transform.TransformDirection(Vector3.forward), out hit, maxFiringDistance, layer, QueryTriggerInteraction.Ignore))
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    laserRenderers[i].SetPosition(1, new Vector3(0, 0, hit.distance));
                    laserColliders[i].transform.localPosition = new Vector3(0, 0, hit.distance / 2);
                    laserColliders[i].transform.localScale = new Vector3(3.5f, 3.5f, hit.distance);
                }
                else
                {
                    laserRenderers[i].SetPosition(1, new Vector3(0, 0, maxFiringDistance));
                    laserColliders[i].transform.localPosition = new Vector3(0, 0, (maxFiringDistance / 2) + 3);
                    laserColliders[i].transform.localScale = new Vector3(3.5f, 3.5f, maxFiringDistance);
                }
                laserAudio[i].volume = SettingsMenu.sfxVolume;
                laserAudio[i].UnPause();
                if (!laserAudio[i].isPlaying)
                    laserAudio[i].Play();
            }
            

            if (laserDTimer <= 0)
            {
                //Turn off laser 
                laserDTimer = laserDuration;
                isFiringLaser = false;
            }
        }
        else
        {
            for (int i = 0; i < laserFirePoints.Length; i++)
            {
                laserRenderers[i].enabled = false;
                laserColliders[i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                laserColliders[i].transform.localPosition = new Vector3(0, 0, 2.5f);
                laserAudio[i].Pause();
            }

            if (laserITimer <= 0)
            {
                //Turn on laser 
                laserITimer = laserInterval;
                isFiringLaser = true;
            }
        }
    }

    IEnumerator ReverseCountdown() {
        while (stuckTimer > 0f) {
            yield return new WaitForSeconds(1);
            stuckTimer -= 1f;
        }
        reverseCourse = false;
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
        float maxHeight = target.transform.position.y + (heightDifference + (deltaTarget / 10f));
        if (this.transform.position.y > maxHeight)
        {
            floatForce.force = new Vector3(0, restingFloatForce, 0);
        }
        else
        {
            floatForce.force = new Vector3(0, upwardFloatForce, 0);
        }
    }

    private void RotateAxis() {
        //Rotating forward transform to direction of the player
        Vector3 direction = target.position - rb.position; 
        direction.Normalize();
        Vector3 rotateToPlayer = Vector3.Cross(transform.forward, direction);
        rotateToPlayer = Vector3.Project(rotateToPlayer, transform.up);
        rb.AddRelativeTorque(rotateToPlayer * rotationForce * 5);

        //Local up to World Up
        Vector3 predictedUp = Quaternion.AngleAxis(
             rb.angularVelocity.magnitude * Mathf.Rad2Deg * rotationForce / force,
             rb.angularVelocity
         ) * transform.up;
        Vector3 uprightBalance = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(uprightBalance * rotationForce * 3);
    }

    private void Move() {
        if (!reverseCourse)
        {
            rb.AddForce(transform.forward * force);
            //Debug.Log(rb.velocity+"--"+rb.velocity.magnitude);
            //Debug.DrawLine(transform.position, transform.forward, Color.green, 1f);
        }
        else
        {
            rb.AddForce(transform.forward*-1 * force);
        }
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
        spineDeath?.Invoke();
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
                child.SetParent(null);
                child.GetComponent<HingeJoint>().breakForce = 0.001f;
                child.GetComponent<Rigidbody>().AddForce(
                    new Vector3(Random.Range(0.001f,0.1f), Random.Range(0.001f, 0.1f), Random.Range(0.001f, 0.1f))
                    );
                child.GetComponent<Pollen_Child>().enabled = true;
            }
        }
    }
}
