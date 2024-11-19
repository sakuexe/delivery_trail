using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Drivetrain
{
    AllWhellDrive,
    FrontWheelDrive,
    RearWheelDrive
}

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

    // references
    private Tire[] powerDeliveryWheels;
    private CarController car;
    private Suspension suspension;

    // states
    private bool _pressingAccelerator;
    private float _gasPedalAmount;
    private float _rpm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();
        suspension = gameObject.GetComponent<Suspension>();

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
        HandleSpeed();
        HandleRpm();
    }

    // when the player presses on the gas
    public void OnGas(InputValue value) => _gasPedalAmount = value.Get<float>();

    /// <summary>
    /// Handles the torque that the powertrain is giving the car
    /// </summary>
    private void HandleSpeed()
    {
        // if the gas pedal is not pressed
        if (_gasPedalAmount <= 0) return;

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
    private void HandleRpm()
    {
        // how much can the _rpm change in a second
        float rpmChangeRate = _gasPedalAmount > 0.2f ? 800 : 1600;

        float desiredRpm = maxRpm * _gasPedalAmount;
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
