using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{

    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed;
    [SerializeField] private float forwardAcceleration;
    [SerializeField] private float boostSpeed;
    private float activeForwardSpeed;
    private float boostTimer;
    [Header("Rolling Movement")]
    [SerializeField] private float rollSpeed;
    [SerializeField] private float rollInput;
    [Header("Look Speed")]
    [SerializeField] private float lookRotateSpeed;
    [SerializeField] private float stationaryRotateSpeed;
    private float defaultLookRotateSpeed;

    private Vector2 lookInput, screenCenter, mouseDistance;
    private Rigidbody rb;

    [Header("Health & Boost")]
    [SerializeField] private int boostFuel;
    [SerializeField] private int boostMax;
    [SerializeField] private int health;
    [SerializeField] private int maxHealth;
    private AudioSource aSource;

    [Header("Booster Effect")]
    [SerializeField] private ParticleSystem boosterEffect;
    [SerializeField] private AudioClip[] engineSounds;
    [SerializeField] private float beStartSpeedBoost;
    [SerializeField] private float beStartSpeedNormal;

    [Header("Fire Bullet")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private AudioClip[] gunFireClips;
    [SerializeField] private AudioSource gunAudioSource;
    private float gunCooldown=0.2f;
    private float gunCDTimer;

    //private float stationaryModeTimer;

    public delegate void OnPlayerInitialized(int boostValue, int healthValuem);
    public static event OnPlayerInitialized initializeUI;

    public delegate void OnFuelChanged(int newBoostValue);
    public static event OnFuelChanged playerFiredGun, playerBoosted;

    public delegate void OnHealthChanged(int newHealthValue);
    public static event OnHealthChanged playerHitByMissile, playerHitByMine, playerHitByLaser,playerHitByRam;

    public delegate void OnScoreIncrement(int scoreIncrement, int updatedFuel);
    public static event OnScoreIncrement balloonCrashPop, shotBalloonPop;

    public delegate void OnPlayerDeath();
    public static event OnPlayerDeath playerHasDied;


    // Start is called before the first frame update
    void Start()
    {
        initializeUI.Invoke(boostFuel, health);
        screenCenter.x = Screen.width*.5f;
        screenCenter.y = Screen.height*.5f;

        rb = GetComponent<Rigidbody>();
        boosterEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        aSource = this.GetComponent<AudioSource>();
        //stationaryModeTimer = 0f;
        defaultLookRotateSpeed = lookRotateSpeed;
        Balloon.balloonPoppedByPCrash += PlayerPoppedDirectly;
        Balloon.balloonPoppedByPShot += PlayerPoppedByShooting;
        boostTimer = 0f;
    }

    private void OnDestroy()
    {
        Balloon.balloonPoppedByPCrash -= PlayerPoppedDirectly;
        Balloon.balloonPoppedByPShot -= PlayerPoppedByShooting;
    }

    private void PlayerPoppedDirectly(int fuelValue, int scoreIncrement) {
        BoostFuelChange(fuelValue);
        balloonCrashPop.Invoke(scoreIncrement, boostFuel);
    }

    private void PlayerPoppedByShooting(int fuelValue, int scoreIncrement) {
        BoostFuelChange(fuelValue);
        int temp = boostFuel + fuelValue;
        shotBalloonPop.Invoke(scoreIncrement, boostFuel);
    }

    // Update is called once per frame
    void Update()
    {
        lookInput.x = Input.mousePosition.x;
        lookInput.y = Input.mousePosition.y;
        mouseDistance.x = (lookInput.x - screenCenter.x)/screenCenter.y;
        mouseDistance.y = (lookInput.y - screenCenter.y)/screenCenter.y;

        mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);
        rollInput = Input.GetAxisRaw("Horizontal") * -1;

        //transform.Rotate(
        //    -mouseDistance.y * lookRotateSpeed * Time.deltaTime,
        //    mouseDistance.x * lookRotateSpeed * Time.deltaTime,
        //    rollInput * rollSpeed * Time.deltaTime, Space.Self);

        float verticalInput = Input.GetAxisRaw("Vertical");

        if (verticalInput > 0 && boostFuel > 0)
        {//Boost
            if (boostTimer == 0
                && activeForwardSpeed == forwardSpeed)
            {
                boostFuel -= 1;
                playerBoosted.Invoke(boostFuel);
            }
            else if (boostTimer >= 1f) {
                boostTimer -= 1f;
                boostFuel -= 1;
                playerBoosted.Invoke(boostFuel);
            }
            boostTimer += Time.deltaTime;

            activeForwardSpeed = Mathf.Lerp(
                    activeForwardSpeed, boostSpeed,
                    forwardAcceleration * Time.deltaTime
                    );

            PlayEngineSoundLoop(1);
            AdjustBoosterEffect(beStartSpeedBoost);
            //stationaryModeTimer = 0;
            lookRotateSpeed = defaultLookRotateSpeed;
        }
        else if (verticalInput == -1)
        {
            activeForwardSpeed = Mathf.Lerp(
                    activeForwardSpeed, 0,
                    forwardAcceleration * Time.deltaTime * 2
                    );
            AdjustBoosterEffect(beStartSpeedNormal);
            //if (stationaryModeTimer == 0)
            //{
            //    boostFuel -= 3;
            //}
            //else {
            //    if (stationaryModeTimer >= 1f) {
            //        boostFuel -= 3;
            //        stationaryModeTimer -= 1f;
            //    }
            //}
            //stationaryModeTimer += Time.deltaTime;
            lookRotateSpeed = stationaryRotateSpeed;
        }
        else {
            //normal
            activeForwardSpeed = Mathf.Lerp(
                    activeForwardSpeed, 1 * forwardSpeed,
                    forwardAcceleration * Time.deltaTime
                    );

            PlayEngineSoundLoop(0);
            AdjustBoosterEffect(beStartSpeedNormal);
            //stationaryModeTimer = 0;
            lookRotateSpeed = defaultLookRotateSpeed;
        }

        if (activeForwardSpeed < 0)
        {//No reverse force allowed
            activeForwardSpeed = 0;
        }

        if ( Time.timeScale > 0 
            && (Input.GetButtonDown("Fire1") || Input.GetButton("Fire1"))
            && (gunCDTimer <= 0) 
            && (boostFuel>0) )
        {
            FireGun();
        }
        else {
            gunCDTimer -= Time.deltaTime;
        }

        //transform.position += (transform.forward * activeForwardSpeed * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        rb.velocity = (transform.forward * activeForwardSpeed);
        //Set the angular velocity of the Rigidbody(rotating around the Y axis, 100 deg / sec)
        Vector3 m_EulerAngleVelocity =
            new Vector3(
                -mouseDistance.y * lookRotateSpeed,
            mouseDistance.x * lookRotateSpeed,
            rollInput * rollSpeed
            );
        Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    private void FireGun() {
        GameObject temp = Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        temp.GetComponent<Rigidbody>().velocity += rb.velocity;
        boostFuel -= 1;
        PlayRandomGunFiringSound();
        playerFiredGun.Invoke(boostFuel);
        gunCDTimer = gunCooldown;
    }

    private void PlayRandomGunFiringSound() {
        int index = UnityEngine.Random.Range(0, gunFireClips.Length);
        gunAudioSource.PlayOneShot(gunFireClips[index]);
    }

    private void PlayEngineSoundLoop(int clipIndex) {
        if (aSource != null)
        {
            if (aSource.clip != engineSounds[clipIndex])
            {
                aSource.clip = engineSounds[clipIndex];
            }
            if (!aSource.isPlaying)
            {
                aSource.Play();
            }
        }
    }

    private void AdjustBoosterEffect(float startSpeed) {
        if (boosterEffect != null)
        {
            var cachedBoosterMain = boosterEffect.main;
            cachedBoosterMain.startSpeed = startSpeed;
        }
    }

    private void BoostFuelChange(int addedValue) {
        boostFuel = boostFuel + addedValue;
        if (boostFuel > 100) { 
            boostFuel = 100; 
        }else if (boostFuel < 0) { 
            boostFuel = 0; 
        }
    }

    public int GetMaxHealth() {
        return maxHealth;
    }

    public void EnemyMineHit(int hpReduction) {
        playerHitByMine.Invoke(health-hpReduction);
        ReduceHealth(hpReduction);
    }

    public void EnemyMissileHit(int hpReduction)
    {
        playerHitByMissile.Invoke(health - hpReduction);
        ReduceHealth(hpReduction);
    }

    public void EnemyLaserHit(int hpReduction)
    {
        playerHitByLaser.Invoke(health - hpReduction);
        ReduceHealth(hpReduction);
    }

    public void EnemyRamHit(int hpReduction) {
        playerHitByRam.Invoke(health - hpReduction);
        ReduceHealth(hpReduction);
    }

    private void ReduceHealth(int amount) {
        health -= amount;
        if (health <= 0) {
            playerHasDied?.Invoke();
        }
    }

    public float GetActiveForwardSpeed() {
        return activeForwardSpeed;
    }

    public float GetBoostMax() {
        return boostMax;
    }

    public Rigidbody GetRB() {
        return rb;
    }
}
