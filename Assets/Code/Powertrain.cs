using System.Linq;
using UnityEngine;

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
    private AnimationCurve powerCurve = new(new Keyframe(0, 0), new Keyframe(15, 100));

    private float speedMultiplier;
    private Tire[] powerDeliveryWheels;

    // references
    private CarController car;

    // states
    private float accelerationStartTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();

        // F_down = mass * gravity
        float carForceDownwards = car.rigidBody.mass * 9.81f;
        // F_acceleration (m/s) = mass * acceleration
        float desiredAcceleration = car.rigidBody.mass * 1;
        // at the end, we count the total force needed to reach the wanted speed (in km/h) from 0 km/h
        // F_total = F_down + (F_acceleration / 3.6)
        speedMultiplier = carForceDownwards + (desiredAcceleration / 3.6f);

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
                powerDeliveryWheels = car.frontTires.Concat(car.rearTires).ToArray();;
                break;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        Accelerate();
    }

    private void Accelerate()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            accelerationStartTime = Time.time;
        }

        if (Input.GetKey(KeyCode.W))
        {
            float accelerationPressedFor = Time.time - accelerationStartTime;
            float forwardForce = powerCurve.Evaluate(accelerationPressedFor);
            forwardForce *= speedMultiplier;
            foreach (Tire tire in powerDeliveryWheels)
            {
                car.rigidBody.AddForceAtPosition(tire.transform.forward * forwardForce * Time.deltaTime, tire.transform.position);
            }
        }
    }

    private void Break()
    {
        if (Input.GetKey(KeyCode.S))
        {
            foreach (Tire tire in car.frontTires)
            {
                float accelerationPressedFor = Time.time - accelerationStartTime;
                float forwardForce = powerCurve.Evaluate(accelerationPressedFor);
                forwardForce *= speedMultiplier;
                car.rigidBody.AddForceAtPosition(-tire.transform.forward * (forwardForce * 2) * Time.deltaTime, tire.transform.position);
            }
        }
    }
}
