using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pollen_Spine : BalloonEnemy
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

    [SerializeField] private Transform[] pollenChildren;

    private float deltaTarget, stuckTimer, stuckInterval = 5f;
    private bool reverseCourse=false;
    private Collision stuckCollision;

    private bool detached = false;

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
        //Debug.Log("before--"+rb.centerOfMass);
        //rb.centerOfMass += new Vector3(-2.4f, 50, 0);
        //rb.WakeUp();
        //Debug.Log("after--"+rb.centerOfMass);
        deltaTarget = Vector3.Distance
                (target.position, this.transform.position);
        stuckTimer = stuckInterval;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;
        if (dead)
        {
            if (!detached)
            {
                InvokeDeathEvent(BalloonEnemyType.PollenSpine);
                DetachChildren();
                detached = true;
            }
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
        if (dead) return;

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
                if(laserRenderers[i]!=null)
                    laserRenderers[i].enabled = false;
                if (laserColliders[i] != null)
                {
                    laserColliders[i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    laserColliders[i].transform.localPosition = new Vector3(0, 0, 2.5f);
                }
                laserAudio[i]?.Pause();
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

    private new void AdjustFloat()
    {
        //Float height target
        float maxHeight = target.transform.position.y + (5 + (deltaTarget / 10f));
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

    new private void Move() {
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

    new public void Explode()
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

    private void DetachChildren() {
        foreach (Transform child in pollenChildren)
        {
            EnemySpawner.enemyNumber += 1;
            child.SetParent(null);
            child.gameObject.GetComponent<HingeJoint>().breakForce = 0.001f;
            child.gameObject.GetComponent<Rigidbody>().AddForce(
                new Vector3(Random.Range(0.001f, 0.1f), Random.Range(0.001f, 0.1f), Random.Range(0.001f, 0.1f))
                );
            child.gameObject.GetComponent<Pollen_Child>().enabled = true;
        }
    }
}
