using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Powertrain))]
[RequireComponent(typeof(Suspension))]
[RequireComponent(typeof(CarController))]
public class Braking : MonoBehaviour
{
    [SerializeField]
    [Range(500, 7_500)]
    private float breakForce = 4_000;
    [SerializeField]
    [Range(0.1f, 1f)]
    // how much wheel bias does the front wheels have
    private float breakBalance = 0.75f;

    private CarController car;
    private Powertrain powertrain;
    private Suspension suspension;

    // states
    private float _brakePressed;

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
    }

    public void OnBrake(InputValue value)
    {
        _brakePressed = value.Get<float>();
    }

    /// <summary>
    /// Applies braking forces to the car based on the amount of brake pedal used.
    /// </summary>
    private void HandleBraking()
    {
        Vector3 worldVelocity = car.rigidBody.GetPointVelocity(car.transform.position);
        float forwardVelocity = Vector3.Dot(car.transform.forward, worldVelocity);

        // if going backward, add force forward and viceversa
        Vector3 decelerationDirection = forwardVelocity < 0 ? car.transform.forward : -car.transform.forward;

        // Acceleration = F_break / mass
        float deceleration = (breakForce * _brakePressed) / car.rigidBody.mass;
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
            car.rigidBody.AddForceAtPosition(decelerationDirection * decelarationForce, tire.transform.position);
        }

        foreach (Tire tire in car.rearTires)
        {
            if (suspension.IsGrounded(tire) == null)
                continue;

            float tireBrakeForce = ((rearBrakeForce) / (totalAmountOfTires - car.frontTires.Length));
            car.rigidBody.AddForceAtPosition(decelerationDirection * decelarationForce, tire.transform.position);
        }
    }
}
