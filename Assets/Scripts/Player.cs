using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

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
    private bool overrideMouse, fullyDisableMouse = false;

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
    [SerializeField] private Animator jetPivot;

    [Header("Fire Bullet")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private AudioClip[] gunFireClips;
    [SerializeField] private AudioSource gunAudioSource;
    private float gunCooldown = 0.2f, gunCDTimer;
    private bool fireInput = false;

    [Header("Player Death")]
    [SerializeField] private GameObject explosionPrefab;
    private bool dead = false;


    private Vector3 dodgeVector = Vector3.zero;
    private float dodgeCooldown = 0.6f, dodgeCDTimer, dodgeInput
        , dodgeDuration = 0.3f, dodgeDurationTimer, cruiseControlInput, dodgeSpeed = 500;
    public short cruiseControl = 0;

    //private float stationaryModeTimer;

    public delegate void OnPlayerInitialized(int boostValue, int healthValuem);
    public static event OnPlayerInitialized initializeUI;

    public enum FuelChangeCause
    {
        FiredGun, Boosted, BalloonPop
    }
    public delegate void OnFuelChanged(int newBoostValue, FuelChangeCause cause);
    public static event OnFuelChanged onFuelUsed, onFuelFilled;

    public enum HealthChangedCause
    {
        EnemyMissile, EnemyMine, EnemyLaser, EnemyRam
    }
    public delegate void OnHealthChanged(int newHealthValue, HealthChangedCause cause);
    public static event OnHealthChanged onHealthChanged;

    public enum ScoreIncrementCause
    {
        FratricideMissile, FratricideMine, FratricideLaser, FratricideRam
    }
    public delegate void OnScoreIncrement(int scoreIncrement, int updatedFuel, ScoreIncrementCause cause);
    public static event OnScoreIncrement fratricide;

    public delegate void OnEngineStateChanged(Manager.EngineState newState);
    public static event OnEngineStateChanged engineStateChanged;


    public delegate void OnPlayerDeath();
    public static event OnPlayerDeath playerHasDied;

    private PlayerInput playerInput;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent <PlayerInput>();
        initializeUI?.Invoke(boostFuel, health);
        screenCenter.x = Screen.width*.5f;
        screenCenter.y = Screen.height*.5f;

        boosterEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        //stationaryModeTimer = 0f;
        defaultLookRotateSpeed = lookRotateSpeed;
        SettingsMenu.mouseMoveToggle += ToggleDisableMouse;
        SettingsMenu.sfxVolumeChange += SFXVolumeChange;
        HomingMine.fratricide += FratricideExplosion;
        EnemyRam.fratricide += FratricideExplosion;
        EnemyLaser.fratricide += FratricideExplosion;

        boostTimer = 0f;
        engineASource.volume = 0;
    }

    public void ReinitializeScreenCenter() {
        screenCenter.x = Screen.width * .5f;
        screenCenter.y = Screen.height * .5f;
    }

    private void OnDestroy()
    {
        SettingsMenu.mouseMoveToggle -= ToggleDisableMouse;
        HomingMine.fratricide -= FratricideExplosion;
        EnemyRam.fratricide -= FratricideExplosion;
        EnemyLaser.fratricide -= FratricideExplosion;
    }

    private void SFXVolumeChange(float newVal) {
        if (engineASource != null)
            engineASource.volume = newVal;
        if(gunAudioSource!=null)
            gunAudioSource.volume = newVal;
    }

    private void FratricideExplosion(ScoreIncrementCause fratricideCause) {
        IncrementBoostFuel(1);
        switch (fratricideCause) {
            case ScoreIncrementCause.FratricideMissile:
                fratricide?.Invoke(40, boostFuel, ScoreIncrementCause.FratricideMissile);
                break;
            case ScoreIncrementCause.FratricideMine:
                fratricide?.Invoke(40, boostFuel, ScoreIncrementCause.FratricideMine);
                break;
            case ScoreIncrementCause.FratricideLaser:
                fratricide?.Invoke(20, boostFuel, ScoreIncrementCause.FratricideLaser);
                break;
            case ScoreIncrementCause.FratricideRam:
                fratricide?.Invoke(40, boostFuel, ScoreIncrementCause.FratricideRam);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) 
        {
            engineASource.volume = 0;
            return; 
        }
        else
        {
            engineASource.volume = SettingsMenu.sfxVolume;
        }

        if (!dead)
        {
            lookInput = playerInput.actions["Pitch & Yaw"].ReadValue<Vector2>();
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
                lookInput = Vector2.ClampMagnitude(mouseDistance, 1f);
            }

            //if (arrowHorizontal != 0 || arrowVertical != 0)
            //{
            //    overrideMouse = true;
            //}
            //else {
            //    overrideMouse = false;
            //}

            rollInput = playerInput.actions["Roll"].ReadValue<float>()*-1;
            verticalInput = playerInput.actions["Acceleration"].ReadValue<float>();

            if ((verticalInput>0 || cruiseControl>0) && boostFuel > 1)
            {//Boost
                if (boostTimer == 0
                    && activeForwardSpeed == forwardSpeed)
                {
                    boostFuel -= 2;
                    onFuelUsed?.Invoke(boostFuel, FuelChangeCause.Boosted);
                }
                else if (boostTimer >= 1f)
                {
                    boostTimer -= 1f;
                    boostFuel -= 2;
                    onFuelUsed?.Invoke(boostFuel, FuelChangeCause.Boosted);
                }
                boostTimer += Time.deltaTime;
                activeForwardSpeed = Mathf.Lerp(
                        activeForwardSpeed, boostSpeed,
                        forwardAcceleration * Time.deltaTime
                        );

                PlayEngineSoundLoop(1);
                AdjustBoosterEffect(beStartSpeedBoost);
                lookRotateSpeed = defaultLookRotateSpeed;
                engineStateChanged?.Invoke(Manager.EngineState.Boost);
                jetPivot.SetBool("down", false);
            }
            else if (verticalInput == -1 || cruiseControl<0)
            {//Stop
                activeForwardSpeed = Mathf.Lerp(
                        activeForwardSpeed, 0,
                        forwardAcceleration * Time.deltaTime * 2
                        );
                AdjustBoosterEffect(beStartSpeedNormal);
                lookRotateSpeed = stationaryRotateSpeed;
                engineStateChanged?.Invoke(Manager.EngineState.Stop);
                jetPivot.SetBool("down", true);
            }
            else
            {//Normal
                cruiseControl = 0;
                activeForwardSpeed = Mathf.Lerp(
                        activeForwardSpeed, 1 * forwardSpeed,
                        forwardAcceleration * 2 * Time.deltaTime
                        );

                PlayEngineSoundLoop(0);
                AdjustBoosterEffect(beStartSpeedNormal);
                lookRotateSpeed = defaultLookRotateSpeed;
                engineStateChanged?.Invoke(Manager.EngineState.Normal);
                jetPivot.SetBool("down", false);
            }

            //Stop cruise control if it detects input in the opposite direction
            if ((cruiseControl > 0 && verticalInput < 0)
                || (cruiseControl < 0 && verticalInput > 0))
            {
                cruiseControl = 0;
            }
            cruiseControlInput = playerInput.actions["CruiseControl"].ReadValue<float>();
            if (cruiseControlInput > 0 && cruiseControl < 1)
            {
                cruiseControl = 1;
            }
            else if (cruiseControlInput < 0 && cruiseControl > -1) 
            {
                cruiseControl = -1;
            }

            if (activeForwardSpeed < 0)
            {//No reverse force allowed
                activeForwardSpeed = 0;
            }

            dodgeInput = playerInput.actions["Dodge"].ReadValue<float>();
            dodgeCDTimer -= Time.deltaTime;
            
            if (dodgeInput != 0 && dodgeCDTimer <= 0)
            {
                dodgeDurationTimer = dodgeDuration;
                dodgeCDTimer = dodgeCooldown;
                dodgeVector = transform.right * dodgeSpeed * dodgeInput;
                Debug.Log("dodge input");
            }

            fireInput = playerInput.actions["Fire"].ReadValue<float>()>0 ? true : false;

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
    }

    private void FixedUpdate()
    {
        if (!dead) { 
            rb.velocity = (transform.forward * activeForwardSpeed + dodgeVector);
            Vector3 m_EulerAngleVelocity =
                new Vector3(
                    -lookInput.y * lookRotateSpeed,
                lookInput.x * lookRotateSpeed,
                rollInput * rollSpeed
                );
            Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);

            if (dodgeVector != Vector3.zero)
            {
                dodgeDurationTimer -= Time.fixedDeltaTime;
            }
            if (dodgeDurationTimer <= 0)
            {
                dodgeVector = Vector3.zero;
                dodgeDurationTimer = dodgeDuration;
            }
        }
    }

    //TODO fix this
    //Sort of done lol
    public void ToggleDisableMouse(bool newState) {
        fullyDisableMouse = newState;
        if (newState)
        {
            InputSystem.DisableDevice(Mouse.current);
            Cursor.visible = false;
            //// Get binding mask for "PC_Scheme_Gamepad_Xbox".
            //string bindingGroup = planeInput.controlSchemes.
            //    First(x => x.name == "Keyboard").bindingGroup;
            //string bindingGroup2 = planeInput.controlSchemes.
            //    First(x => x.name == "Gamepad").bindingGroup;
            //String[] bindingGs = { bindingGroup, bindingGroup2 };
            //// Set as binding mask on actions. What this does is cause any binding that doesn't
            //// match the mask to be ignored. So, by setting the binding mask to that of the "PC_Scheme_Gamepad_Xbox"
            //// group (whose mask name will default to just "PC_Scheme_Gamepad_Xbox" so probably don't even need to
            //// look up the name like above), only bindings in that control scheme will be used.
            //planeInput.bindingMask = InputBinding.MaskByGroups(bindingGs);
        }
        else
        {
            InputSystem.EnableDevice(Mouse.current);
            Cursor.visible = true;
            //planeInput.bindingMask = null;
        }
    }

    private void FireGun() {
        GameObject temp = Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        temp.GetComponent<Rigidbody>().velocity += rb.velocity;
        boostFuel -= 1;
        PlayRandomGunFiringSound();
        gunCDTimer = gunCooldown;
        onFuelUsed?.Invoke(boostFuel, FuelChangeCause.FiredGun);
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

    public void IncrementBoostFuel(int addedValue) {
        boostFuel = boostFuel + addedValue;
        if (boostFuel > 100) { 
            boostFuel = 100; 
        }else if (boostFuel < 0) { 
            boostFuel = 0; 
        }
        onFuelFilled?.Invoke(boostFuel, FuelChangeCause.BalloonPop);
    }


    public void HitByEnemy(HealthChangedCause cause) {
        switch (cause) {
            case HealthChangedCause.EnemyMine:
                onHealthChanged?.Invoke(health - 1, HealthChangedCause.EnemyMine);
                ReduceHealth(1);
                break;
            case HealthChangedCause.EnemyMissile:
                onHealthChanged?.Invoke(health - 1, HealthChangedCause.EnemyMissile);
                ReduceHealth(1);
                break;
            case HealthChangedCause.EnemyLaser:
                onHealthChanged?.Invoke(health - 1, HealthChangedCause.EnemyLaser);
                ReduceHealth(1);
                break;
            case HealthChangedCause.EnemyRam:
                onHealthChanged?.Invoke(health - 2, HealthChangedCause.EnemyRam);
                ReduceHealth(2);
                if ((activeForwardSpeed - 100) < 0)
                    activeForwardSpeed = 0;
                else
                    activeForwardSpeed -= 100;
                break;
        }
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
    public int GetBoostFuel() { return boostFuel; }

    public int GetMaxHealth() { return maxHealth; }
    public float GetBoostMax() { return boostMax;}

    public Rigidbody GetRB() { return rb;}
}
