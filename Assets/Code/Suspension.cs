using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class Suspension : MonoBehaviour
{
    [Header("Suspension Settings")]
    [SerializeField]
    private float springStiffness = 8000;
    [SerializeField]
    // the value should be between (2 190 - 10 000)
    private float damperStiffness = 5000;
    // the length of the spring when it is not moving (in meters)
    public float restLength = 0.75f;
    // how long much can the spring move back and forth
    public float springTravel = 0.4f;

    private CarController car;
    private Tire[] tires;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();
        tires = car.frontTires.Concat(car.rearTires).ToArray();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleSuspension();
    }

    private void HandleSuspension()
    {
        foreach (Tire tire in tires)
        {
            Transform rayPoint = tire.transform;
            RaycastHit hit;

            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + tire.radius, car.driveableLayer))
            {
                float currentSpringLength = hit.distance - tire.radius;
                // how much the spring has compressed from the neutral position
                float springOffset = restLength - currentSpringLength / springTravel;

                // to calculate the dampening force, we need to know the current velocity of the spring
                float springVelocity = Vector3.Dot(car.rigidBody.GetPointVelocity(rayPoint.position), rayPoint.up);

                // F_damp = velocity * dampening
                float dampForce = springVelocity * damperStiffness;

                // F_spring = offset * strength
                // the offset is the distance form the resting position of the spring
                // the strength is the firmness of the spring
                float springForce = springOffset * springStiffness;

                // F_total = (offset * strength) - (velocity * dampening)
                float totalForce = springForce - dampForce;

                car.rigidBody.AddForceAtPosition(totalForce * rayPoint.up, rayPoint.position);

                // draw lines to visualize the spring force
                Debug.DrawLine(rayPoint.position, hit.point, Color.red);
            }
            else
            {
                Debug.DrawLine(rayPoint.position, rayPoint.position + (tire.radius + maxLength) * -rayPoint.up, Color.green);
            }
        }
    }
}
