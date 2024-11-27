using UnityEngine;

[RequireComponent(typeof(Powertrain))]
[RequireComponent(typeof(Suspension))]
[RequireComponent(typeof(CarController))]
public class Braking : MonoBehaviour
{
    [Header("Braking")]
    [SerializeField]
    [Range(2_000, 12_500)]
    private float breakForce = 6_000;
    [SerializeField]
    [Range(0.1f, 1f)]
    // how much wheel bias does the front wheels have
    private float breakBalance = 0.75f;

    [Header("Reversing")]
    public float maxReversingSpeed = 20;

    private CarController car;
    private Powertrain powertrain;
    private Suspension suspension;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();
        powertrain = gameObject.GetComponent<Powertrain>();
        suspension = gameObject.GetComponent<Suspension>();
    }

    /// <summary>
    /// Applies braking forces to the car based on the amount of brake pedal used.
    /// </summary>
    public void Brake(float brakePedalAmount)
    {
        Vector3 worldVelocity = car.rigidBody.GetPointVelocity(car.transform.position);
        float forwardVelocity = Vector3.Dot(car.transform.forward, worldVelocity);

        // if going backward, add force forward and viceversa
        Vector3 decelerationDirection = forwardVelocity < 0 ? car.transform.forward : -car.transform.forward;

        // Acceleration = F_break / mass
        float deceleration = (breakForce * brakePedalAmount) / car.rigidBody.mass;
        // Force = mass * acceleration
        float decelarationForce = car.rigidBody.mass * deceleration;

        // calculate how much force is given to the front axel
        float frontBrakeForce = decelarationForce * breakBalance;
        float rearBrakeForce = breakForce - frontBrakeForce;

        int totalAmountOfTires = car.frontTires.Length + car.rearTires.Length;

        foreach (Tire tire in car.frontTires)
        {
            if (suspension.IsGrounded(tire) == null)
                continue;

            float tireBrakeForce = (frontBrakeForce / (totalAmountOfTires - car.rearTires.Length));
            car.rigidBody.AddForceAtPosition(decelerationDirection * frontBrakeForce, tire.transform.position);
        }

        foreach (Tire tire in car.rearTires)
        {
            if (suspension.IsGrounded(tire) == null)
                continue;

            float tireBrakeForce = ((rearBrakeForce) / (totalAmountOfTires - car.frontTires.Length));
            car.rigidBody.AddForceAtPosition(decelerationDirection * rearBrakeForce, tire.transform.position);
        }
    }

    /// <summary>
    /// Reverses the car when the brake button is pressed and the car is still.
    /// </summary>
    public void Reverse()
    {
        float torque = car.powertrain.GetCurrentTorque() * 10f;
        if (powertrain.drivetrain == Drivetrain.AllWhellDrive)
            torque = torque / 4;
        else
            torque = torque / 2;

        foreach (Tire tire in car.powertrain.powerDeliveryWheels)
        {
            // dont accelerate if the wheel is off the ground
            if (suspension.IsGrounded(tire) == null)
                continue;
            float force = torque / tire.radius;
            car.rigidBody.AddForceAtPosition(-tire.transform.forward * force, tire.transform.position);
        }
    }
}
