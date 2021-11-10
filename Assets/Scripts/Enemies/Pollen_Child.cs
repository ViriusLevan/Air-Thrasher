using AirThrasher.Assets.Scripts.UserInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AirThrasher.Assets.Scripts.Enemies
{
    public class Pollen_Child : BalloonEnemy
    {
        [Header("Laser Firing")]
        [SerializeField] private GameObject laserCollider;
        [SerializeField] private Transform laserFirePoint;
        [SerializeField] private LineRenderer laserRenderer;
        [SerializeField] private float laserInterval;
        [SerializeField] private float laserDuration;
        [SerializeField] private float maxFiringDistance;
        [SerializeField] private AudioSource laserAudio;
        private float laserITimer = 0f, laserDTimer = 0f;
        private bool isFiringLaser = false;


        private bool active = false;
        private float startDelay = 3f;
        [SerializeField] private Animator balloonAnimator;

        // Start is called before the first frame update
        void Start()
        {
            floatForce = GetComponent<ConstantForce>();
            laserITimer = laserInterval;
            laserDTimer = laserDuration;
            rb = GetComponent<Rigidbody>();


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
            if (Time.timeScale == 0) return;
            if (active)
            {
                if (dead)
                {
                    explosionTimer -= Time.deltaTime;
                    if (explosionTimer <= 0)
                    {
                        InvokeDeathEvent(BalloonEnemyType.PollenChild);
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
            else
            {
                startDelay -= Time.deltaTime;
                if (startDelay <= 0)
                {
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
                    (target.position, transform.position)
                    > maxFiringDistance)
                {
                    Move();
                }
                ToggleLaser();
            }
        }

        private void Activate()
        {
            active = true;
            balloonAnimator.SetBool("deflated", false);
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
                    laserCollider.transform.localPosition = new Vector3(0, 0, maxFiringDistance / 2 + 3);
                }
                laserAudio.volume = SettingsMenu.sfxVolume;
                laserAudio.UnPause();
                if (!laserAudio.isPlaying)
                {
                    laserAudio.Play();
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
                laserRenderer.enabled = false;
                laserCollider.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                laserCollider.transform.localPosition = new Vector3(0, 0, 2.5f);
                laserAudio.Pause();
                if (laserITimer <= 0)
                {
                    //Turn on laser 
                    laserITimer = laserInterval;
                    isFiringLaser = true;
                }
            }
        }
    }
}