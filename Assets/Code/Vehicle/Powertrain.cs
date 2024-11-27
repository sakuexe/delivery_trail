using System.Linq;
using UnityEngine;

public enum Drivetrain
{
    AllWhellDrive,
    FrontWheelDrive,
    RearWheelDrive
}

[RequireComponent(typeof(Braking))]
[RequireComponent(typeof(CarController))]
public class Powertrain : MonoBehaviour
{
    [SerializeField]
    public Drivetrain drivetrain = Drivetrain.FrontWheelDrive;
    [SerializeField]
    [Range(200, 800)]
    private int minRpm = 500;
    [SerializeField]
    [Range(3_500, 8_000)]
    private int maxRpm = 6_000;
    [SerializeField]
    [Range(80, 1_000)]
    private int maxHorsePower = 200;
    [SerializeField]
    // keyframes are (rpm, power)
    private AnimationCurve powerCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    [Range(0.1f, 5)]
    private float engineBraking = 1.75f;

    // references
    public Tire[] powerDeliveryWheels { get; private set; }
    private CarController car;
    private Suspension suspension;
    private Braking braking;

    // states
    private bool _pressingAccelerator;
    private float _rpm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();
        suspension = gameObject.GetComponent<Suspension>();
        braking = gameObject.GetComponent<Braking>();

        // get the wheels that are doing the driving
        switch (drivetrain)
        {
            case Drivetrain.FrontWheelDrive:
                powerDeliveryWheels = car.frontTires;
                break;
            case Drivetrain.RearWheelDrive:
                powerDeliveryWheels = car.rearTires;
                break;
            case Drivetrain.AllWhellDrive:
                powerDeliveryWheels = car.frontTires.Concat(car.rearTires).ToArray();
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleEngineBrake();
    }

    /// <summary>
    /// Handles the engine braking amount when the player is not pressing the accelerator
    /// </summary>
    private void HandleEngineBrake()
    {
        if (_pressingAccelerator) return;
        if (car.isReversing) return;

        // if going backward, add force forward and viceversa
        Vector3 worldVelocity = car.rigidBody.GetPointVelocity(car.transform.position);
        float forwardVelocity = Vector3.Dot(car.transform.forward, worldVelocity);
        Vector3 decelerationDirection = forwardVelocity < 0 ? car.transform.forward : -car.transform.forward;

        // Force = mass * acceleration
        float decelarationForce = (car.rigidBody.mass * engineBraking); 
        // apply a bit extra engine brake based on the rpm (more brake on high rpm)
        decelarationForce += decelarationForce * (powerCurve.Evaluate(_rpm / maxRpm) * 3);

        // apply the braking to each tire
        foreach (Tire tire in powerDeliveryWheels)
        {
            float tireBrakeForce = (decelarationForce / (powerDeliveryWheels.Length));
            car.rigidBody.AddForceAtPosition(decelerationDirection * tireBrakeForce, tire.transform.position);
            // for debugging
            Debug.DrawLine(tire.transform.position, tire.transform.position + (-tire.transform.forward * (tireBrakeForce / 2000)), Color.green);
        }
    }

    /// <summary>
    /// Accelerates the car by adding torque from the powertrain to the wheels
    /// </summary>
    /// <param name="gasPedalAmount">
    /// The amount of gas pressed. The value gets clamped between 0 and 1
    /// </param>
    public void Accelerate(float gasPedalAmount)
    {
        _pressingAccelerator = true;
        // make sure that the value is between 0 and 1
        gasPedalAmount = Mathf.Clamp(gasPedalAmount, 0, 1);

        // split the torque across the wheels
        // TODO: add a better multiplier
        float torque = GetCurrentTorque() * 10f;
        if (drivetrain == Drivetrain.AllWhellDrive)
            torque = torque / 4;
        else
            torque = torque / 2;

        foreach (Tire tire in powerDeliveryWheels)
        {
            // dont accelerate if the wheel is off the ground
            if (suspension.IsGrounded(tire) == null)
                continue;
            float force = torque / tire.radius;
            car.rigidBody.AddForceAtPosition(tire.transform.forward * force, tire.transform.position);
        }
    }

    /// <summary>
    /// Handles counting the _rpm and makes sure that it cannot cannot go above or below the max and min values.
    /// </summary>
    public void HandleRpm(float gasPedalAmount)
    {
        if (gasPedalAmount <= 0) 
            _pressingAccelerator = false;
        // how much can the _rpm change in a second
        float rpmChangeRate = gasPedalAmount > 0.2f ? 800 : 1600;

        float desiredRpm = maxRpm * gasPedalAmount;
        float currentRpm = Mathf.MoveTowards(_rpm, desiredRpm, rpmChangeRate * Time.deltaTime);

        // do not exceed min and max _rpm values
        _rpm = Mathf.Clamp(currentRpm, minRpm, maxRpm);
    }

    /// <summary>
    /// Get the current torque of the powertain.
    /// </summary>
    public float GetCurrentTorque()
    {
        // HP = (T * _rpm) / 5252
        // since we know the horsepower and the _rpm and want to know the torque
        // we can do some math flips and get the formula for torque like this
        // Torque = (HP * 5252) / _rpm

        // get the horsepower from the _rpm curve
        // use normalized _rpm (so that max _rpm is 1 and min is 0), because the curve is 1 by 1
        // this way we get a factor that can count the percentage of max HP used
        float horsePowerFactor = powerCurve.Evaluate(_rpm / maxRpm);
        // and then get the horse power from the _rpm
        float horsePower = maxHorsePower * horsePowerFactor;
        // use the formula above to get torque
        float torque = (horsePower * 5252) / Mathf.Max(_rpm, minRpm);
        return torque;
    }

    /// <summary>
    /// Get the current speed of the vehicle with this powertrain attached.
    /// The speed is given in km/h.
    /// </summary>
    public float GetCurrentSpeed()
    {
        // speed in m/s
        float speed = car.rigidBody.linearVelocity.magnitude;
        // speed in km/h
        return speed * 3.6f;
    }

    /// <summary>
    /// Get the current rpm of the powertrain.
    /// </summary>
    public float GetCurrentRpm() => _rpm;
}
