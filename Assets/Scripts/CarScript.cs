using UnityEngine;
using UnityEngine.Assertions;

public class CarScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Rigidbody carRb;
    [SerializeField]
    private Transform[] rayPoints;
    [SerializeField]
    private LayerMask driveable;

    [Header("Suspension Settings")]
    [SerializeField]
    private float springStiffness;
    [SerializeField]
    private float damperStiffness;
    [SerializeField]
    private float restLength;
    [SerializeField]
    private float springTravel;
    [SerializeField]
    private float wheelRadius;

    [Header("Speed Settings")]
    [SerializeField]
    private float speed = 100;
    [SerializeField]
    private float steeringSpeed = 100;

    // makes sure that the values passed to speed are close to km/h
    private float speedMultiplier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (carRb == null)
            carRb = GetComponent<Rigidbody>();
        // check that the rigidbody is not null, throw error if it is
        Assert.IsNotNull(carRb);

        // F_down = mass * gravity
        float carForceDownwards = carRb.mass * 9.81f;
        // F_acceleration (m/s) = mass * acceleration
        float desiredAcceleration = carRb.mass * 1;
        // at the end, we count the total force needed to reach the wanted speed (in km/h) from 0 km/h
        // F_total = F_down + (F_acceleration / 3.6)
        speedMultiplier = carForceDownwards + (desiredAcceleration / 3.6f);
    }

    void FixedUpdate()
    {
        Suspension();
        Control();
    }

    private void Control()
    {
        if (Input.GetKey(KeyCode.W))
        {
            carRb.AddRelativeForce(Vector3.forward * (speed * speedMultiplier) * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            carRb.AddRelativeForce(Vector3.left * steeringSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            carRb.AddRelativeForce(Vector3.right * steeringSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            carRb.AddRelativeForce(Vector3.back * (speed * speedMultiplier * 2) * Time.deltaTime);
        }
    }

    private void Suspension()
    {
        foreach (Transform rayPoint in rayPoints)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + wheelRadius, driveable))
            {
                float currentSpringLength = hit.distance - wheelRadius;
                // how much the spring has compressed from the neutral position
                float springOffset = restLength - currentSpringLength / springTravel;

                // to calculate the dampening force, we need to know the current velocity of the spring
                float springVelocity = Vector3.Dot(carRb.GetPointVelocity(rayPoint.position), rayPoint.up);

                // F_damp = velocity * dampening
                float dampForce = springVelocity * damperStiffness;

                // F_spring = offset * strength
                // the offset is the distance form the resting position of the spring
                // the strength is the firmness of the spring
                float springForce = springOffset * springStiffness;

                // F_total = (offset * strength) - (velocity * dampening)
                float totalForce = springForce - dampForce;

                carRb.AddForceAtPosition(totalForce * rayPoint.up, rayPoint.position);

                // draw lines to visualize the spring force
                Debug.DrawLine(rayPoint.position, hit.point, Color.red);
            }
            else
            {
                Debug.DrawLine(rayPoint.position, rayPoint.position + (wheelRadius + maxLength) * -rayPoint.up, Color.green);
            }
        }
    }
}
