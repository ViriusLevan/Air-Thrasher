using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{

    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed = 50f;
    [SerializeField] private float activeForwardSpeed;
    [SerializeField] private float forwardAcceleration = 5f;
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float boostMaxSpeed = 75f;
    [Header("Rolling Movement")]
    [SerializeField] private float rollSpeed = 20f;
    [SerializeField] private float rollInput;
    [SerializeField] private float rollAcceleration = 3.5f;
    [Header("Look Speed")]
    [SerializeField] private float lookRotateSpeed = 90f;

    private Vector2 lookInput, screenCenter, mouseDistance;
    private Rigidbody rb;

    [Header("Health & Boost")]
    [SerializeField] private float boostFuel = 0;
    [SerializeField] private float boostMax = 100f;
    [SerializeField] private int health = 10;
    [SerializeField] private int maxHealth = 10;
    private AudioSource aSource;

    [Header("Booster Effect")]
    [SerializeField] private ParticleSystem boosterEffect;
    [SerializeField] private AudioClip[] engineSounds;
    [SerializeField] private float beStartSpeedBoost;
    [SerializeField] private float beStartSpeedNormal;

    public delegate void OnBalloonPopped();
    public static event OnBalloonPopped balloonPopped;

    public delegate void OnPlayerDeath();
    public static event OnPlayerDeath playerHasDied;

    // Start is called before the first frame update
    void Start()
    {
        screenCenter.x = Screen.width*.5f;
        screenCenter.y = Screen.height*.5f;
        Cursor.lockState = CursorLockMode.Confined;

        rb = GetComponent<Rigidbody>();
        boosterEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        aSource = this.GetComponent<AudioSource>();
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
            activeForwardSpeed = Mathf.Lerp(
                    activeForwardSpeed, 2 * forwardSpeed,
                    forwardAcceleration * 2 * Time.deltaTime
                    );

            PlayEngineSoundLoop(1);
            AdjustBoosterEffect(beStartSpeedBoost);
            
        }
        else {//normal
            activeForwardSpeed = Mathf.Lerp(
                    activeForwardSpeed, 1 * forwardSpeed,
                    forwardAcceleration * Time.deltaTime
                    );

            PlayEngineSoundLoop(0);
            AdjustBoosterEffect(beStartSpeedNormal);
        }

        if (activeForwardSpeed < 0)
        {//No brakes/reverse force allowed
            activeForwardSpeed = 0;
        }

        //transform.position += (transform.forward * activeForwardSpeed * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        rb.velocity = (transform.forward * activeForwardSpeed);
        //rb.AddForce(transform.forward * activeForwardSpeed * forwardSpeed, ForceMode.Acceleration);
        //Debug.Log(transform.forward * activeForwardSpeed * forwardSpeed + " -> " + rb.velocity);
        ////rb.AddForce(transform.up * activeHoverSpeed, ForceMode.Acceleration);

        //Set the angular velocity of the Rigidbody(rotating around the Y axis, 100 deg / sec)
        Vector3 m_EulerAngleVelocity =
            new Vector3(
                -mouseDistance.y * lookRotateSpeed,
            mouseDistance.x * lookRotateSpeed,
            rollInput * rollSpeed
            );
        Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);

        //if (rb.velocity.magnitude > maxSpeed)
        //{
        //    rb.velocity = rb.velocity.normalized * maxSpeed;
        //}
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
    public void BalloonPoppedByPlane() {
        balloonPopped?.Invoke();
        IncreaseBoostFuel(10);
    }

    public void IncreaseBoostFuel(float amount) {
        boostFuel += amount;
        if (boostFuel > 100) { 
            boostFuel = 100; 
        }
    }

    public int GetHealth() {
        return health;
    }

    public int GetMaxHealth() {
        return maxHealth;
    }

    public void ReduceHealth(int amount) {
        health -= amount;
        if (health <= 0) {
            playerHasDied?.Invoke();
        }
    }

    public float GetActiveForwardSpeed() {
        return activeForwardSpeed;
    }

    public float GetBoostFuel() {
        return boostFuel;
    }

    public float GetBoostMax() {
        return boostMax;
    }

    public Rigidbody GetRB() {
        return rb;
    }
}
