using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public float forwardSpeed = 50f,  hoverSpeed = 5f;
    //strafeSpeed = 7.5f,
    public float activeForwardSpeed;
    private float activeHoverSpeed;
    //activeStrafeSpeed,
    private float forwardAcceleration = 5f, hoverAcceleration = 2f;
    //strafeAcceleration = 2f, 
    public float lookRotateSpeed = 90f;
    private Vector2 lookInput, screenCenter, mouseDistance;
    //private Rigidbody rb;
    public float boostFuel=0, boostMax=100f;

    private float rollInput;
    public float rollSpeed=20f, rollAcceleration=3.5f, maxSpeed = 50f, boostMaxSpeed=75f;
    [SerializeField] private ParticleSystem boosterEffect;
    private int health=10, maxHealth=10;
    private AudioSource aSource;
    [SerializeField]private AudioClip[] engineSounds;

    // Start is called before the first frame update
    void Start()
    {
        screenCenter.x = Screen.width*.5f;
        screenCenter.y = Screen.height*.5f;

        Cursor.lockState = CursorLockMode.Confined;
        boosterEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        //rb = GetComponent<Rigidbody>();
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
        rollInput = Mathf.Lerp(
            rollInput, Input.GetAxisRaw("Horizontal") *-1 * rollSpeed,
            rollAcceleration * Time.deltaTime
            );

        transform.Rotate(
            -mouseDistance.y * lookRotateSpeed * Time.deltaTime,
            mouseDistance.x * lookRotateSpeed * Time.deltaTime,
            rollInput * rollSpeed * Time.deltaTime, Space.Self);

        float verticalInput = Input.GetAxisRaw("Vertical");


        if (verticalInput > 0 && boostFuel > 0)
        {//Boost
            activeForwardSpeed = Mathf.Lerp(
                    activeForwardSpeed, 2 * forwardSpeed,
                    forwardAcceleration * 2 * Time.deltaTime
                    );
            boostFuel -= Time.deltaTime;
            
            if (aSource != null) {
                if (aSource.clip != engineSounds[1])
                {
                    aSource.clip = engineSounds[1];
                }
                if (!aSource.isPlaying) {
                    aSource.Play();
                }
            }

            if (boosterEffect != null)
            {
                var cachedBoosterMain = boosterEffect.main;
                cachedBoosterMain.startSpeed = 7;
            }
        }
        else {//normal
            activeForwardSpeed = Mathf.Lerp(
                    activeForwardSpeed, 1 * forwardSpeed,
                    forwardAcceleration * Time.deltaTime
                    );

            if (aSource != null)
            {
                if (aSource.clip != engineSounds[0])
                {
                    aSource.clip = engineSounds[0];
                }
                if (!aSource.isPlaying)
                {
                    aSource.Play();
                }
            }

            if (boosterEffect != null)
            {
                var cachedBoosterMain = boosterEffect.main;
                cachedBoosterMain.startSpeed = 3;
            }
        }

        if (activeForwardSpeed < 0)
        {
            activeForwardSpeed = 0;
        }

        //activeStrafeSpeed = Mathf.Lerp(
        //    activeStrafeSpeed, Input.GetAxisRaw("Horizontal") * strafeSpeed,
        //    strafeAcceleration * Time.deltaTime
        //    );
        activeHoverSpeed = Mathf.Lerp(
            activeHoverSpeed, Input.GetAxisRaw("Hover") * hoverSpeed,
            hoverAcceleration * Time.deltaTime
            );


        transform.position += (transform.forward * activeForwardSpeed * Time.deltaTime)
            + (transform.up * activeHoverSpeed * Time.deltaTime);
        //+ (transform.right * activeStrafeSpeed * Time.deltaTime)
        

        //activeForwardSpeed = Input.GetAxisRaw("Vertical") * forwardSpeed;

        //if (activeForwardSpeed < 0)
        //{//no brakes/reverse force allowed
        //    activeForwardSpeed = 0;
        //}

        ////activeHoverSpeed = Input.GetAxisRaw("Hover") * hoverSpeed;
    }
    private void FixedUpdate()
    {
        //if (boosterEffect != null)
        //{//Adjusting particle effect based on 'forward' key press
        //    var cachedBoosterMain = boosterEffect.main;
        //    if (Input.GetAxisRaw("Vertical") > 0)
        //        cachedBoosterMain.startSpeed = 7;
        //    else
        //        cachedBoosterMain.startSpeed = 3;
        //}

        //Vector3 accelForce = new Vector3(activeForwardSpeed, activeHoverSpeed, 0);
        //rb.AddForce(transform.forward*activeForwardSpeed, ForceMode.Acceleration);
        //rb.AddForce(transform.up * activeHoverSpeed, ForceMode.Acceleration);

        //transform.Rotate(
        //    -mouseDistance.y * lookRotateSpeed * Time.deltaTime, 
        //    mouseDistance.x * lookRotateSpeed * Time.deltaTime, 
        //    rollInput * rollSpeed * Time.deltaTime, Space.Self);

        //Set the angular velocity of the Rigidbody (rotating around the Y axis, 100 deg/sec)
        //Vector3 m_EulerAngleVelocity = 
        //    new Vector3(
        //        -mouseDistance.y * lookRotateSpeed, 
        //    mouseDistance.x * lookRotateSpeed, 
        //    rollInput * rollSpeed
        //    );
        //Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
        //rb.MoveRotation(rb.rotation * deltaRotation);

        //if (rb.velocity.magnitude > maxSpeed) {
        //    rb.velocity = rb.velocity.normalized * maxSpeed;
        //}
    }

    public delegate void OnBalloonPopped();
    public static event OnBalloonPopped balloonPopped;

    public delegate void OnPlayerDeath();
    public static event OnPlayerDeath playerHasDied;

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
            playerHasDied.Invoke();
        }
    }
}
