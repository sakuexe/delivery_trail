using System.Linq;
using TMPro;
using UnityEngine;

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
    private TMP_Text rpmUI;
    [SerializeField]
    private TMP_Text kphUI;
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
        HandleAcceleration();
        HandleRpm();
        HandleBreaking();
        UpdatePowerTrainUI();
    }

    private void HandleAcceleration()
    {
        if (Input.GetKeyDown(KeyCode.W))
            pressingAccelerator = true;

        if (Input.GetKeyUp(KeyCode.W))
            pressingAccelerator = false;

        if (!Input.GetKey(KeyCode.W))
            return;

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
        float changePerSecond = 800;

        if (pressingAccelerator)
        {
            float desiredRpm = rpm + changePerSecond * Time.deltaTime;
            // do not exceed min and max rpm values
            rpm = Mathf.Clamp(desiredRpm, minRpm, maxRpm);
        }
        else
        {
            float desiredRpm = rpm - changePerSecond * Time.deltaTime;
            // do not exceed min and max rpm values
            rpm = Mathf.Clamp(desiredRpm, minRpm, maxRpm);
        }
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
        rpmUI.text = $"RPM: {rpm.ToString("0")}";
        kphUI.text = $"Speed: {(GetCurrentSpeed()).ToString("0")} km/h";
    }
}
