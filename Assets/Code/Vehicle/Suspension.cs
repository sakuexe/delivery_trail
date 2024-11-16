using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class Suspension : MonoBehaviour
{
    [Header("Suspension Settings")]
    [SerializeField]
    [UnityEngine.Range(2_000, 20_000)]
    private float springStiffness = 12_000;
    [SerializeField]
    [UnityEngine.Range(0.2f, 1.5f)]
    private float damperStiffnessZeta = 0.5f;
    // the length of the spring when it is not moving (in meters)
    [UnityEngine.Range(0.2f, 4f)]
    public float restLength = 1.3f;
    // how long much can the spring move back and forth
    [UnityEngine.Range(0.1f, 2f)]
    public float springTravel = 0.7f;

    // this will be calculated using the zeta value
    private float damperStiffness => CalculateDamperStiffness();

    private CarController car;
    private Tire[] tires;
    private Transform[] tireModels;
    private Transform[] initialTireModelPositions;

    void Awake()
    {
        car = gameObject.GetComponent<CarController>();
        tires = car.frontTires.Concat(car.rearTires).ToArray();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    /// <summary>
    /// Calculates the damperStiffness based on the given zeta value
    /// </summary>
    private float CalculateDamperStiffness()
    {
        // DamperStiffness = (2 * sqrt(SpringStiffness * CarMass) * Zeta
        // we give the zeta value, so that we can change the stiffness easily
        // and keep it still accurate and realistic
        return (2 * Mathf.Sqrt(springStiffness * car.rigidBody.mass)) * damperStiffnessZeta;
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
