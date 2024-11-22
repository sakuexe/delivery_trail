using UnityEngine;
using UnityEngine.InputSystem;

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
    private float maxReversingSpeed = 20;

    private CarController car;
    private Powertrain powertrain;
    private Suspension suspension;

    // states
    public float brakePressed { get; private set; }
    public bool isReversing { get; private set; } = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();
        powertrain = gameObject.GetComponent<Powertrain>();
        suspension = gameObject.GetComponent<Suspension>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleBraking();
        HandleReverse();
    }

    public void OnBrake(InputValue value) => brakePressed = value.Get<float>();
    public void OnGas(InputValue value) => isReversing = false;

    /// <summary>
    /// Applies braking forces to the car based on the amount of brake pedal used.
    /// </summary>
    private void HandleBraking()
    {
        if (brakePressed <= 0) return;
        if (isReversing) return;

        Vector3 worldVelocity = car.rigidBody.GetPointVelocity(car.transform.position);
        float forwardVelocity = Vector3.Dot(car.transform.forward, worldVelocity);

        // if going backward, add force forward and viceversa
        Vector3 decelerationDirection = forwardVelocity < 0 ? car.transform.forward : -car.transform.forward;

        // Acceleration = F_break / mass
        float deceleration = (breakForce * brakePressed) / car.rigidBody.mass;
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
    private void HandleReverse()
    {
        if (brakePressed <= 0) return;
        if (powertrain.GetCurrentSpeed() > maxReversingSpeed) return;

        Vector3 worldVelocity = car.rigidBody.GetPointVelocity(car.transform.position);
        float forwardVelocity = Vector3.Dot(car.transform.forward, worldVelocity);

        if (forwardVelocity > 0) return;

        isReversing = true;

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
