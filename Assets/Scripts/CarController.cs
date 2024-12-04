using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody theRB;

    public float fowardAccel = 5f,
        reverseAccel = 3f,
        maxSpeed = 35f,
        turnStrength = 180,
        gravityForce = 10f,
        dragOnGround = 3f;

    private float speedInput,
        turnInput;

    private bool grounded;

    public LayerMask whatIsGround;
    public float groundRayLength = 0.5f;
    public Transform groundRayPoint;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;
    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f;
    private float emissionRate;

    void Start()
    {
        theRB.transform.parent = null;
    }

    void Update()
    {
        speedInput = 0f;
        turnInput = 0f;

        if (Input.GetAxis("Vertical") > 0)
        {
            speedInput = Input.GetAxis("Vertical") * fowardAccel * 1000f;
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAccel * 1000f;
        }

        leftFrontWheel.localEulerAngles = new Vector3(
            leftFrontWheel.localEulerAngles.x,
            (Input.GetAxis("Horizontal") * maxWheelTurn) - 180,
            leftFrontWheel.localEulerAngles.z
        );

        rightFrontWheel.localEulerAngles = new Vector3(
            rightFrontWheel.localEulerAngles.x,
            (Input.GetAxis("Horizontal") * maxWheelTurn),
            rightFrontWheel.localEulerAngles.z
        );

        turnInput = Input.GetAxis("Horizontal");

        if (grounded)
        {
            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles
                    + new Vector3(
                        0f,
                        turnInput * turnStrength * Time.deltaTime * Input.GetAxis("Vertical"),
                        0f
                    )
            );
        }

        transform.position = theRB.transform.position;
    }

    private void FixedUpdate()
    {
        grounded = false;
        RaycastHit hit;

        if (
            Physics.Raycast(
                groundRayPoint.position,
                -transform.up,
                out hit,
                groundRayLength,
                whatIsGround
            )
        )
        {
            grounded = true;
            transform.rotation = Quaternion.FromToRotation(
                transform.up,
                hit.normal
            ) * transform.rotation;
        }

        emissionRate = 0f;

        if (grounded)
        {
            theRB.drag = dragOnGround;
            if (Mathf.Abs(speedInput) > 0)
            {
                theRB.AddForce(transform.forward * speedInput);
                emissionRate = maxEmission;
            }
        }
        else
        {
            theRB.drag = 0.1f;
            theRB.AddForce(Vector3.up * -gravityForce * 200f); // Increased gravity force
        }

        foreach (ParticleSystem part in dustTrail)
        {
            var emissionModule = part.emission;
            emissionModule.rateOverTime = emissionRate;
        }

        if (Mathf.Abs(speedInput) > 0)
        {
            theRB.AddForce(transform.forward * speedInput);

            if (theRB.velocity.magnitude > maxSpeed)
            {
                theRB.velocity = theRB.velocity.normalized * maxSpeed;
            }
        }

        if (Mathf.Abs(turnInput) > 0)
        {
            theRB.rotation = Quaternion.Euler(
                theRB.rotation.eulerAngles
                    + new Vector3(0f, turnInput * turnStrength * Time.deltaTime, 0f)
            );
        }
    }
}