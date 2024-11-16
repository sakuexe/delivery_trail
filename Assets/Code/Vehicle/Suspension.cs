using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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
    private Transform[] tireModels;
    private Transform[] initialTireModelPositions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();
        tires = car.frontTires.Concat(car.rearTires).ToArray();

        Transform tiresContainer = transform.Find("Model/Tires");
        // throw error if tires transform is not found
        Assert.IsNotNull(tiresContainer);

        HashSet<Transform> transforms = new(tiresContainer.GetComponentsInChildren<Transform>());
        transforms.Remove(tiresContainer.transform);
        tireModels = transforms.ToArray();
        initialTireModelPositions = tireModels;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleSuspension();
    }

    void Update()
    {
        KeepTiresGrounded();
    }

    private void HandleSuspension()
    {
        foreach (Tire tire in tires)
        {
            RaycastHit? tireHitGround = IsGrounded(tire);

            if (tireHitGround == null)
            {
                float maxLength = restLength + springTravel;
                Debug.DrawLine(tire.transform.position, tire.transform.position + (tire.radius + maxLength) * -tire.transform.up, Color.green);
                continue;
            }

            RaycastHit hit = tireHitGround.GetValueOrDefault();

            float currentSpringLength = hit.distance - tire.radius;
            // how much the spring has compressed from the neutral position
            float springOffset = restLength - currentSpringLength / springTravel;

            // to calculate the dampening force, we need to know the current velocity of the spring
            float springVelocity = Vector3.Dot(car.rigidBody.GetPointVelocity(tire.transform.position), tire.transform.up);

            // F_damp = velocity * dampening
            float dampForce = springVelocity * damperStiffness;

            // F_spring = offset * strength
            // the offset is the distance form the resting position of the spring
            // the strength is the firmness of the spring
            float springForce = springOffset * springStiffness;

            // F_total = (offset * strength) - (velocity * dampening)
            float totalForce = springForce - dampForce;

            car.rigidBody.AddForceAtPosition(totalForce * tire.transform.up, tire.transform.position);

            // draw lines to visualize the spring force
            Debug.DrawLine(tire.transform.position, hit.point, Color.red);
        }
    }

    public RaycastHit? IsGrounded(Tire tire)
    {
        RaycastHit hit;
        float springMaxLength = restLength + springTravel;

        if (Physics.Raycast(tire.transform.position, -tire.transform.up, out hit, springMaxLength + tire.radius, car.driveableLayer))
            return hit;

        return null;
    }

    /// <summary>
    /// A function for making the tire models hit the ground.
    /// This way you can always adjust the springs without worrying about tires floating.
    /// </summary>
    private void KeepTiresGrounded()
    {
        foreach (var (tireModel, index) in tireModels.Select((v, i) => (v, i)))
        {
            RaycastHit hit;
            Tire currentTire = tires[index];
            float maxLength = restLength + springTravel + currentTire.radius;
            float yTransform;

            // if the tire can reach the ground
            if (Physics.Raycast(currentTire.transform.position, Vector3.down, out hit, maxLength, car.driveableLayer))
                yTransform = hit.point.y + currentTire.radius;
            else
                yTransform = maxLength;

            // only move on the y axis (up and down)
            Vector3 desiredPosition = new Vector3(
                    initialTireModelPositions[index].position.x,
                    yTransform, 
                    initialTireModelPositions[index].position.z);

            tireModel.position = Vector3.MoveTowards(tireModel.position, desiredPosition, 2f * Time.deltaTime);
        }
    }
}
