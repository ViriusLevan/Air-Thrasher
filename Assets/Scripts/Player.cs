using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using System.Linq;

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
    [Header("Look Speed")]
    [SerializeField] private float lookRotateSpeed;
    [SerializeField] private float stationaryRotateSpeed;
    private float defaultLookRotateSpeed;
    private float arrowHorizontal, arrowVertical;
    private bool overrideMouse, fullyDisableMouse=false;

    private float verticalInput, rollInput;
    private Vector2 lookInput, screenCenter, mouseDistance;
    private Rigidbody rb;

    [Header("Health & Boost")]
    [SerializeField] private int boostFuel;
    [SerializeField] private int boostMax;
    [SerializeField] private int health;
    [SerializeField] private int maxHealth;
    [SerializeField] private AudioSource engineASource;

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
    private bool fireInput = false;

    [Header("Player Death")]
    [SerializeField] private GameObject explosionPrefab;
    private bool dead = false;

    //private float stationaryModeTimer;

    public delegate void OnPlayerInitialized(int boostValue, int healthValuem);
    public static event OnPlayerInitialized initializeUI;

    public delegate void OnFuelChanged(int newBoostValue);
    public static event OnFuelChanged playerFiredGun, playerBoosted;

    public delegate void OnHealthChanged(int newHealthValue);
    public static event OnHealthChanged playerHitByMissile, playerHitByMine, playerHitByLaser,playerHitByRam;

    public delegate void OnScoreIncrement(int scoreIncrement, int updatedFuel);
    public static event OnScoreIncrement 
        balloonCrashPop, shotBalloonPop, friendlyMissilePop,
        enemyLaserExplosion, enemyMineExplosion, enemyRamExplosion;

    public delegate void OnPlayerDeath();
    public static event OnPlayerDeath playerHasDied;
    #region Input
    //public void InputPitchYaw(InputAction.CallbackContext ctx)
    //{
    //    var inputValue = ctx.ReadValue<Vector2>();
    //    lookInput = new Vector2(inputValue.x, inputValue.y);
    //}
    //public void InputBoostBrake(InputAction.CallbackContext ctx)
    //{
    //    var inputValue = ctx.ReadValue<float>();
    //    verticalInput = inputValue;
    //}
    //public void InputRoll(InputAction.CallbackContext ctx)
    //{
    //    var inputValue = ctx.ReadValue<float>();
    //    rollInput = inputValue * -1;
    //}
    //public void InputFire(InputAction.CallbackContext ctx)
    //{
    //    fireInput = true;
    //}
    #endregion

    private Controls_Plane planeInput;

    private void Awake()
    {
        planeInput = new Controls_Plane();
        planeInput.Gameplay.Enable();
        //planeInput.Gameplay.PitchYaw.performed += InputPitchYaw;
        //planeInput.Gameplay.BoostBrake.performed += InputBoostBrake;
        //planeInput.Gameplay.Roll.performed += InputRoll;
        //planeInput.Gameplay.Fire.performed += InputFire;
    }

    // Start is called before the first frame update
    void Start()
    {
        initializeUI?.Invoke(boostFuel, health);
        screenCenter.x = Screen.width*.5f;
        screenCenter.y = Screen.height*.5f;

        rb = GetComponent<Rigidbody>();
        boosterEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        //stationaryModeTimer = 0f;
        defaultLookRotateSpeed = lookRotateSpeed;
        SettingsMenu.mouseMoveToggle += ToggleDisableMouse;
        SettingsMenu.sfxVolumeChange += SFXVolumeChange;
        Balloon.balloonPoppedByPCrash += PlayerPoppedDirectly;
        Balloon.balloonPoppedByPShot += PlayerPoppedByShooting;
        Balloon.balloonPoppedByFMissile += PoppedByFriendlyMissile;
        EnemyLaser.laserExplosion += ExplosionByLaser;
        EnemyRam.ramExplosion += ExplosionByRam;
        HomingMine.mineExplosion += ExplosionByMine;
        
        boostTimer = 0f;
        engineASource.volume = SettingsMenu.sfxVolume;
    }

    public void ReinitializeScreenCenter() {
        screenCenter.x = Screen.width * .5f;
        screenCenter.y = Screen.height * .5f;
    }

    private void OnDestroy()
    {
        SettingsMenu.mouseMoveToggle -= ToggleDisableMouse;
        Balloon.balloonPoppedByPCrash -= PlayerPoppedDirectly;
        Balloon.balloonPoppedByPShot -= PlayerPoppedByShooting;
        Balloon.balloonPoppedByFMissile -= PoppedByFriendlyMissile;
        EnemyLaser.laserExplosion -= ExplosionByLaser;
        EnemyRam.ramExplosion -= ExplosionByRam;
        HomingMine.mineExplosion -= ExplosionByMine;
    }

    private void SFXVolumeChange(float newVal) {
        if (engineASource != null)
            engineASource.volume = newVal;
        if(gunAudioSource!=null)
            gunAudioSource.volume = newVal;
    }

    private void PlayerPoppedDirectly(int fuelValue, int scoreIncrement) {
        BoostFuelChange(fuelValue);
        balloonCrashPop?.Invoke(scoreIncrement, boostFuel);
    }

    private void PlayerPoppedByShooting(int fuelValue, int scoreIncrement) {
        BoostFuelChange(fuelValue);
        shotBalloonPop?.Invoke(scoreIncrement, boostFuel);
    }
    private void PoppedByFriendlyMissile(int fuelValue, int scoreIncrement)
    {
        BoostFuelChange(fuelValue);
        friendlyMissilePop?.Invoke(scoreIncrement, boostFuel);
    }

    private void ExplosionByMine()
    {
        BoostFuelChange(1);
        enemyMineExplosion?.Invoke(40,boostFuel);
    }

    private void ExplosionByRam()
    {
        BoostFuelChange(1);
        enemyRamExplosion?.Invoke(40, boostFuel);
    }

    private void ExplosionByLaser()
    {
        BoostFuelChange(1);
        enemyLaserExplosion?.Invoke(20, boostFuel);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0 && !dead)
        {
            //lookInput.x = Input.mousePosition.x;
            //lookInput.y = Input.mousePosition.y;
            lookInput = planeInput.Gameplay.PitchYaw.ReadValue<Vector2>();
            //Debug.Log(lookInput);
            if (fullyDisableMouse)
            {
                lookInput = Vector2.ClampMagnitude(lookInput, 1f);
            }
            else
            {
                mouseDistance.x =
                    (lookInput.x - screenCenter.x) / screenCenter.y;
                mouseDistance.y =
                    (lookInput.y - screenCenter.y) / screenCenter.y;
                //mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);
                lookInput = Vector2.ClampMagnitude(mouseDistance, 1f);
            }
            Debug.Log(lookInput);
            //arrowHorizontal = Input.GetAxisRaw("ArrowHorizontal");
            //arrowVertical = Input.GetAxisRaw("ArrowVertical");

            //if (arrowHorizontal != 0 || arrowVertical != 0)
            //{
            //    overrideMouse = true;
            //}
            //else {
            //    overrideMouse = false;
            //}

            rollInput = planeInput.Gameplay.Roll.ReadValue<float>()*-1;
            verticalInput = planeInput.Gameplay.BoostBrake.ReadValue<float>();
            //rollInput = Input.GetAxisRaw("Horizontal") * -1;
            //float verticalInput = Input.GetAxisRaw("Vertical");

            if (verticalInput > 0 && boostFuel > 0)
            {//Boost
                if (boostTimer == 0
                    && activeForwardSpeed == forwardSpeed)
                {
                    boostFuel -= 2;
                    playerBoosted?.Invoke(boostFuel);
                }
                else if (boostTimer >= 1f)
                {
                    boostTimer -= 1f;
                    boostFuel -= 2;
                    playerBoosted?.Invoke(boostFuel);
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
            {//Stop
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
            else
            {//Normal
                activeForwardSpeed = Mathf.Lerp(
                        activeForwardSpeed, 1 * forwardSpeed,
                        forwardAcceleration * 2 * Time.deltaTime
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

            fireInput = planeInput.Gameplay.Fire.ReadValue<float>()>0 ? true : false;

            if (fireInput
                && (gunCDTimer <= 0)
                && (boostFuel > 0))
            {
                FireGun();
            }
            else
            {
                gunCDTimer -= Time.deltaTime;
            }
        }
        //transform.position += (transform.forward * activeForwardSpeed * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        if (!dead) { 
            rb.velocity = (transform.forward * activeForwardSpeed);

            //Set the angular velocity of the Rigidbody(rotating around the Y axis, 100 deg / sec)
            //if (overrideMouse || fullyDisableMouse)
            //{
            //    Vector3 m_EulerAngleVelocity2 =
            //        new Vector3(
            //            -arrowVertical * lookRotateSpeed,
            //        arrowHorizontal * lookRotateSpeed,
            //        rollInput * rollSpeed
            //        );

            //    Quaternion deltaRotation2 = Quaternion.Euler(m_EulerAngleVelocity2 * Time.fixedDeltaTime);
            //    rb.MoveRotation(rb.rotation * deltaRotation2);
            //}
            //else
            //{
            Vector3 m_EulerAngleVelocity =
                new Vector3(
                    -lookInput.y * lookRotateSpeed,
                lookInput.x * lookRotateSpeed,
                rollInput * rollSpeed
                );

            Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);
            //}

            //Debug.Log(lookInput);
        }
    }

    public void ToggleDisableMouse(bool newState) {
        fullyDisableMouse = newState;
        if (newState)
        {
            // Get binding mask for "PC_Scheme_Gamepad_Xbox".
            string bindingGroup = planeInput.controlSchemes.
                First(x => x.name == "Keyboard").bindingGroup;
            string bindingGroup2 = planeInput.controlSchemes.
                First(x => x.name == "Gamepad").bindingGroup;
            String[] bindingGs = { bindingGroup, bindingGroup2 };
            // Set as binding mask on actions. What this does is cause any binding that doesn't
            // match the mask to be ignored. So, by setting the binding mask to that of the "PC_Scheme_Gamepad_Xbox"
            // group (whose mask name will default to just "PC_Scheme_Gamepad_Xbox" so probably don't even need to
            // look up the name like above), only bindings in that control scheme will be used.
            planeInput.bindingMask = InputBinding.MaskByGroups(bindingGs);
        }
        else {
            planeInput.bindingMask = null;
        }
    }

    private void FireGun() {
        GameObject temp = Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        temp.GetComponent<Rigidbody>().velocity += rb.velocity;
        boostFuel -= 1;
        PlayRandomGunFiringSound();
        gunCDTimer = gunCooldown;
        playerFiredGun?.Invoke(boostFuel);
    }

    private void PlayRandomGunFiringSound() {
        int index = UnityEngine.Random.Range(0, gunFireClips.Length);
        gunAudioSource.PlayOneShot(gunFireClips[index]);
    }

    private void PlayEngineSoundLoop(int clipIndex) {
        if (engineASource != null)
        {
            if (engineASource.clip != engineSounds[clipIndex])
            {
                engineASource.volume = SettingsMenu.sfxVolume;
                engineASource.clip = engineSounds[clipIndex];
            }
            if (!engineASource.isPlaying)
            {
                engineASource.Play();
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
        playerHitByMine?.Invoke(health-hpReduction);
        ReduceHealth(hpReduction);
    }

    public void EnemyMissileHit(int hpReduction)
    {
        playerHitByMissile?.Invoke(health - hpReduction);
        ReduceHealth(hpReduction);
    }

    public void EnemyLaserHit(int hpReduction)
    {
        playerHitByLaser?.Invoke(health - hpReduction);
        ReduceHealth(hpReduction);
    }

    public void EnemyRamHit(int hpReduction) {
        playerHitByRam?.Invoke(health - hpReduction);
        ReduceHealth(hpReduction);
        if ((activeForwardSpeed - 100) < 0)
            activeForwardSpeed = 0;
        else
            activeForwardSpeed -= 100;
    }


    private void ReduceHealth(int amount) {
        health -= amount;
        if (health <= 0) {
            health = 0;
            dead = true;
            StartCoroutine(PlayDeathAnimation());
        }
    }

    private IEnumerator PlayDeathAnimation()
    {
        AdjustBoosterEffect(0);
        //"Animation" is just an explosion for now lol
        Instantiate(explosionPrefab, 
            transform.position 
            + new Vector3(UnityEngine.Random.Range(-3, 3), 
            UnityEngine.Random.Range(-2, 2), 
            UnityEngine.Random.Range(-1, 1))
            , transform.rotation);
        yield return new WaitForSeconds(0.5f);
        Instantiate(explosionPrefab,
            transform.position
            + new Vector3(UnityEngine.Random.Range(-3, 3),
            UnityEngine.Random.Range(-2, 2),
            UnityEngine.Random.Range(-1, 1))
            , transform.rotation);
        yield return new WaitForSeconds(0.5f);
        Instantiate(explosionPrefab,
            transform.position
            + new Vector3(UnityEngine.Random.Range(-3, 3),
            UnityEngine.Random.Range(-2, 2),
            UnityEngine.Random.Range(-1, 1))
            , transform.rotation);
        yield return new WaitForSeconds(0.2f);
        playerHasDied?.Invoke();
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
