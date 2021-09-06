using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _TestCube : MonoBehaviour
{

    public float forwardSpeed = 50f;
    public float lookRotateSpeed = 90f;
    private Vector2 lookInput, screenCenter, mouseDistance;
    private float rollInput;
    public float rollSpeed = 20f, rollAcceleration = 3.5f, maxSpeed = 50f, boostMaxSpeed = 75f;
    public float activeForwardSpeed;
    private float forwardAcceleration = 5f, hoverAcceleration = 2f;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        screenCenter.x = Screen.width * .5f;
        screenCenter.y = Screen.height * .5f;

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        lookInput.x = Input.mousePosition.x;
        lookInput.y = Input.mousePosition.y;
        mouseDistance.x = (lookInput.x - screenCenter.x) / screenCenter.y;
        mouseDistance.y = (lookInput.y - screenCenter.y) / screenCenter.y;

        mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);
        rollInput = Mathf.Lerp(
            rollInput, Input.GetAxisRaw("Horizontal") * -1 * rollSpeed,
            rollAcceleration * Time.deltaTime
            );

        //transform.Rotate(
        //    -mouseDistance.y * lookRotateSpeed * Time.deltaTime,
        //    mouseDistance.x * lookRotateSpeed * Time.deltaTime,
        //    rollInput * rollSpeed * Time.deltaTime, Space.Self);

        float verticalInput = Input.GetAxisRaw("Vertical");

        if (verticalInput > 0)
        {//Boost
            //activeForwardSpeed = Mathf.Lerp(
            //        activeForwardSpeed, 2 * forwardSpeed,
            //        forwardAcceleration * 2 * Time.deltaTime
            //        );
            activeForwardSpeed = 2f;
        }
        else
        {//normal
            //activeForwardSpeed = Mathf.Lerp(
            //        activeForwardSpeed, 1 * forwardSpeed,
            //        forwardAcceleration * Time.deltaTime
            //        );
            activeForwardSpeed = 1f;
        }

        if (activeForwardSpeed < 0)
        {
            activeForwardSpeed = 0;
        }

    }
    private void FixedUpdate()
    {
        rb.AddForce(transform.forward * activeForwardSpeed * forwardSpeed, ForceMode.Acceleration);
        Debug.Log(transform.forward * activeForwardSpeed * forwardSpeed + " -> " + rb.velocity);
        //rb.AddForce(transform.up * activeHoverSpeed, ForceMode.Acceleration);

        //Set the angular velocity of the Rigidbody(rotating around the Y axis, 100 deg / sec)
        Vector3 m_EulerAngleVelocity =
            new Vector3(
                -mouseDistance.y * lookRotateSpeed,
            mouseDistance.x * lookRotateSpeed,
            rollInput * rollSpeed
            );
        Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);

        //if (rb.velocity.magnitude > maxSpeed) {
        //    rb.velocity = rb.velocity.normalized * maxSpeed;
        //}
    }
}
