using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public record TireAxel
{
    public Transform left;
    public Transform right;
    public float tireGripFactor;
    public float tireMass;

    public Transform[] tires => new Transform[]{left, right};
}

public class CarScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Rigidbody carRb;
    [SerializeField]
    private TireAxel frontTires;
    [SerializeField]
    private TireAxel rearTires;
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
    private Transform[] allRayPoints;

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

        // get all the tire raypoints for suspension usage
        allRayPoints = new Transform[]{
            frontTires.left,
            frontTires.right,
            rearTires.left,
            rearTires.right,
        };
    }

    void FixedUpdate()
    {
        Suspension();
        /*carRb.AddRelativeForce(Vector3.forward *15000);*/
        Acceleration();
        Steering();
    }

    private void Acceleration()
    {
        if (Input.GetKey(KeyCode.W))
        {
            foreach (Transform tire in frontTires.tires)
            {
                carRb.AddForceAtPosition(tire.forward * (speed * speedMultiplier) * Time.deltaTime, tire.position);
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            foreach (Transform tire in frontTires.tires)
            {
                carRb.AddForceAtPosition(-tire.forward * (speed * speedMultiplier * 3) * Time.deltaTime, tire.position);
            }
        }

        if (Input.GetKey(KeyCode.A))
        {
            foreach (Transform tire in frontTires.tires)
            {
                Quaternion rotation = Quaternion.Euler(0, -45, 0);
                tire.localRotation = rotation;
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            foreach (Transform tire in frontTires.tires)
            {
                Quaternion rotation = Quaternion.Euler(0, 45, 0);
                tire.localRotation = rotation;
            }
        }
        else
        {
            foreach (Transform tire in frontTires.tires)
            {
                Quaternion rotation = Quaternion.Euler(0, 0, 0);
                tire.localRotation = rotation;
            }
        }
    }

    private void Steering() 
    {
        foreach (Transform rayPoint in frontTires.tires)
        {
            // draw lines to visualize the spring force
            Debug.DrawLine(rayPoint.position, rayPoint.position + (rayPoint.forward * 10), Color.blue);
            Debug.DrawLine(rayPoint.position, rayPoint.position + (rayPoint.up * 10), Color.blue);

            RaycastHit hit;
            float maxLength = restLength + springTravel;

            // check if the car is not on the ground
            if (!Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + wheelRadius, driveable))
            {
                Debug.Log("car is not on the ground");
                return;
            }

            Vector3 steeringDirection = rayPoint.right;
            if (Input.GetKey(KeyCode.A))
                steeringDirection = -rayPoint.right;

            Vector3 tireWorldVelocity = carRb.GetPointVelocity(rayPoint.position);
            Debug.DrawLine(rayPoint.position, rayPoint.position + (tireWorldVelocity * 10), Color.magenta);
            Debug.DrawLine(rayPoint.position, rayPoint.position + (steeringDirection * 10), Color.green);

            float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVelocity);

            float desiredVelocityChange = -steeringVelocity * frontTires.tireGripFactor;

            float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

            Vector3 netForce = steeringDirection * desiredAcceleration * frontTires.tireMass;
            carRb.AddForceAtPosition(netForce, rayPoint.position);
        }
    }

    private void Suspension()
    {
        foreach (Transform rayPoint in allRayPoints)
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
