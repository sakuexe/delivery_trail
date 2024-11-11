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
    // keyframes are (rpm, power)
    private AnimationCurve powerCurve = new(new Keyframe(500, 0), new Keyframe(5000, 250));
    [SerializeField]
    private int minRpm = 500;
    [SerializeField]
    private int maxRpm = 5000;
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
        Break();
    }

    // Handles rpm and makes sure that it cannot cannot go above or below the max and min values
    private void HandleRpm()
    {
        float changePerSecond = 1000;
        if (pressingAccelerator)
        {
            rpm = Mathf.Clamp((rpm + changePerSecond * Time.deltaTime), minRpm, maxRpm);
        }
        else
        {
            rpm = Mathf.Clamp((rpm - changePerSecond * Time.deltaTime), minRpm, maxRpm);
        }
        rpmUI.text = $"RPM: {rpm.ToString("0")}";
        // speed in m/s
        float speed = car.rigidBody.linearVelocity.magnitude;
        kphUI.text = $"Speed: {(speed * 3.6f).ToString("0")} km/h";
    }

    private float GetCurrentForce()
    {
        // HP = (T * rpm) / 5252
        // since we know the horsepower and the rpm and want to know the torque
        // we can do some math flips and get the formula for torque like this
        // Torque = (HP * 5252) / rpm

        // get the horsepower from the rpm curve
        float horsePower = powerCurve.Evaluate(rpm);
        // use the formula above
        float torque = (horsePower * 5252) / rpm;
        // convert it to pounds
        float forceInPounds = torque / 2;
        // and finally, convert it to newtons
        return forceInPounds * 4.44822f;
    }

    private void HandleAcceleration()
    {
        if (Input.GetKeyDown(KeyCode.W))
            pressingAccelerator = true;

        if (Input.GetKeyUp(KeyCode.W))
            pressingAccelerator = false;

        if (!Input.GetKey(KeyCode.W))
            return;

        foreach (Tire tire in powerDeliveryWheels)
        {
            // dont accelerate if the wheel is off the ground
            if (suspension.IsGrounded(tire) == null)
                continue;
            car.rigidBody.AddForceAtPosition(tire.transform.forward * GetCurrentForce(), tire.transform.position);
        }
    }

    private void Break()
    {
        if (!Input.GetKey(KeyCode.S))
            return;

        foreach (Tire tire in car.frontTires)
            car.rigidBody.AddForceAtPosition(-tire.transform.forward * (breakForce * Time.deltaTime), tire.transform.position);
    }
}
