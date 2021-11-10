using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AirThrasher.Assets.Scripts.Enemies
{
    public class Barge : BalloonEnemy
    {
        //[Header("Cannon Firing")]
        //[SerializeField] private GameObject bargeBullet;
        //[SerializeField] private Transform[] cannonPointsLeft;
        //[SerializeField] private Transform[] cannonPointsRight;
        //[SerializeField] private float firingInterval;
        //[SerializeField] private float maxFiringDistance;
        //private float timer = 0f;

        [SerializeField] private ParticleSystem[] smokeEmitters;
        private float smokeTimer, smokeCooldown = 15f, smokeDuration = 5f;
        private enum EmitterState
        {
            emitting, cooldown
        }
        private EmitterState smokeState;

        private float deltaTarget;
        private float deltaLeftTarget, deltaRightTarget;

        // Start is called before the first frame update
        void Start()
        {
            smokeState = EmitterState.cooldown;
            floatForce = GetComponent<ConstantForce>();
            //timer = firingInterval;
            rb = GetComponent<Rigidbody>();
            if (target == null)
            {
                target = GameObject.FindGameObjectWithTag("Player").transform;
            }
            deltaTarget = Vector3.Distance
                    (target.position, transform.position);
            deltaRightTarget = Vector3.Angle(transform.right, target.position);
            deltaLeftTarget = Vector3.Angle(transform.right * -1, target.position);
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
                    InvokeDeathEvent(BalloonEnemyType.Barge);
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
                smokeTimer -= Time.deltaTime;
                if (smokeTimer < 0)
                {
                    if (smokeState == EmitterState.cooldown)
                    {
                        EmitSmoke();
                    }
                    else
                    {
                        TurnOffSmoke();
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (!dead)
            {
                AdjustFloat();
                Turn();
                deltaTarget = Vector3.Distance
                        (target.position, transform.position);
                deltaRightTarget = Vector3.Angle(transform.right, target.position);
                deltaLeftTarget = Vector3.Angle(transform.right * -1, target.position);
                Move();
            }
        }

        private void EmitSmoke()
        {
            smokeTimer = smokeDuration;
            smokeState = EmitterState.emitting;
            foreach (ParticleSystem ps in smokeEmitters)
            {
                ps.Play();
            }
        }

        private void TurnOffSmoke()
        {
            smokeTimer = smokeCooldown;
            smokeState = EmitterState.cooldown;
            foreach (ParticleSystem ps in smokeEmitters)
            {
                ps.Stop();
            }
        }

        //private void FireCannons()
        //{
        //    if (timer <= 0)
        //    {
        //        if (deltaRightTarget < deltaLeftTarget)
        //        {
        //            timer = firingInterval;
        //            foreach (Transform sPoint in cannonPointsRight)
        //            {
        //                Instantiate(bargeBullet, sPoint.position, sPoint.rotation);
        //            }
        //        }
        //        else
        //        {
        //            timer = firingInterval;
        //            foreach (Transform sPoint in cannonPointsLeft)
        //            {
        //                Instantiate(bargeBullet, sPoint.position, sPoint.rotation);
        //            }
        //        }
        //    }
        //}

        new protected void Move()
        {
            if (rb.velocity.magnitude < 100)
            {
                rb.AddForce(transform.forward * force);
            }
        }

        new protected void AdjustFloat()
        {
            //Float height target
            if (transform.position.y - 3 >= target.position.y)
            {
                floatForce.force = new Vector3(0, restingFloatForce, 0);
            }
            else
            {
                floatForce.force = new Vector3(0, upwardFloatForce, 0);
            }
        }


        //new private void Turn()
        //{
        //    Vector3 direction = target.position - rb.position; //getting direction
        //    direction.Normalize(); //erasing magnitude
        //    Vector3 rotationAmount = new Vector3();
        //    if (deltaTarget <= maxFiringDistance)
        //    {
        //        if (deltaRightTarget < deltaLeftTarget)
        //        {
        //            rotationAmount = Vector3.Cross(transform.right, direction);//calculating angle
        //        }
        //        else {
        //            rotationAmount = Vector3.Cross(transform.right*-1, direction);//calculating angle
        //        }
        //    }
        //    else
        //    {
        //        rotationAmount = Vector3.Cross(transform.forward, direction);//calculating angle
        //    }
        //    rb.AddRelativeTorque(rotationAmount * rotationForce);
        //    //Debug.Log(rotationAmount +"->"+rb.angularVelocity);
        //}

    }
}