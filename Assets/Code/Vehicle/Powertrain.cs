using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Drivetrain
{
    AllWhellDrive,
    FrontWheelDrive,
    RearWheelDrive
}

[RequireComponent(typeof(Suspension))]
[RequireComponent(typeof(CarController))]
public class Powertrain : MonoBehaviour
{
    [SerializeField]
    public Drivetrain drivetrain = Drivetrain.FrontWheelDrive;
    [SerializeField]
    private int minRpm = 500;
    [SerializeField]
    private int maxRpm = 5000;
    [SerializeField]
    private int maxHorsePower = 250;
    [SerializeField]
    // keyframes are (rpm, power)
    private AnimationCurve powerCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    // the breakforce in newtons (1 kgf ~= 9.8067 N)
    [SerializeField]
    private int breakForce = 8000;

    private Tire[] powerDeliveryWheels;

    // references
    private CarController car;
    private Suspension suspension;

    // states
    private bool pressingAccelerator;
    private float _gasPedalAmount;
    private float rpm;

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
    void Update()
    {
        HandleSpeed();
        HandleRpm();
        HandleBreaking();
        UpdatePowerTrainUI();
    }

    // when the player presses on the gas
    public void OnGas(InputValue value) => _gasPedalAmount = value.Get<float>();

    private void HandleSpeed()
    {
        // if the gas pedal is not pressed
        if (_gasPedalAmount <= 0) return;
        // split the force across the wheels
        float force = GetCurrentForce();
        if (drivetrain == Drivetrain.AllWhellDrive)
            force = force / 4;
        else
            force = force / 2;

        foreach (Tire tire in powerDeliveryWheels)
        {
            // dont accelerate if the wheel is off the ground
            if (suspension.IsGrounded(tire) == null)
                continue;
            car.rigidBody.AddForceAtPosition(tire.transform.forward * GetCurrentForce(), tire.transform.position);
        }
    }

    private void HandleBreaking()
    {
        if (!Input.GetKey(KeyCode.S))
            return;

        foreach (Tire tire in car.frontTires)
            car.rigidBody.AddForceAtPosition(-tire.transform.forward * (breakForce * Time.deltaTime), tire.transform.position);
    }


    // Handles rpm and makes sure that it cannot cannot go above or below the max and min values
    private void HandleRpm()
    {
        // how much can the rpm change in a second
        float rpmChangeRate = _gasPedalAmount > 0.2f ? 800 : 1600;

        float desiredRpm = maxRpm * _gasPedalAmount;
        float currentRpm = Mathf.MoveTowards(rpm, desiredRpm, rpmChangeRate * Time.deltaTime);

        // do not exceed min and max rpm values
        rpm = Mathf.Clamp(currentRpm, minRpm, maxRpm);
    }

    private float GetCurrentForce()
    {
        // HP = (T * rpm) / 5252
        // since we know the horsepower and the rpm and want to know the torque
        // we can do some math flips and get the formula for torque like this
        // Torque = (HP * 5252) / rpm

        // get the horsepower from the rpm curve
        // use normalized rpm (so that max rpm is 1 and min is 0), because the curve is 1 by 1
        // this way we get a factor that can count the percentage of max HP used
        float horsePowerFactor = powerCurve.Evaluate(rpm / maxRpm);
        // and then get the horse power from the rpm
        float horsePower = maxHorsePower * horsePowerFactor;
        // use the formula above to get torque
        float torque = (horsePower * 5252) / rpm;
        // convert torque to pounds and then to newtons
        return (torque / 2) * 4.44822f;
    }


    public float GetCurrentSpeed()
    {
        // speed in m/s
        float speed = car.rigidBody.linearVelocity.magnitude;
        // speed in km/h
        return speed * 3.6f;
    }

    private void UpdatePowerTrainUI()
    {
        UIManager.Instance.UpdateRPM(rpm);
        UIManager.Instance.UpdateSpeed(GetCurrentSpeed());
    }
}
