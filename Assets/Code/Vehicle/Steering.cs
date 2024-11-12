using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Suspension))]
[RequireComponent(typeof(Powertrain))]
[RequireComponent(typeof(CarController))]
public class Steering : MonoBehaviour
{
    [SerializeField]
    public AnimationCurve steeringCurve = new(new Keyframe(0, 45), new Keyframe(150, 12));

    private Powertrain powertrain;
    // references
    private CarController car;
    private Suspension suspension;
    private Tire[] tires;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();
        suspension = gameObject.GetComponent<Suspension>();
        powertrain = gameObject.GetComponent<Powertrain>();
        tires = car.frontTires.Concat(car.rearTires).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        HandleSteering();
        HandleGrip();
    }

    private void HandleSteering()
    {
        if (Input.GetKey(KeyCode.A))
        {
            float speed = powertrain.GetCurrentSpeed();
            foreach (Tire tire in car.frontTires)
            {
                Quaternion rotation = Quaternion.Euler(0, -steeringCurve.Evaluate(speed), 0);
                tire.transform.localRotation = rotation;
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            float speed = powertrain.GetCurrentSpeed();
            foreach (Tire tire in car.frontTires)
            {
                Quaternion rotation = Quaternion.Euler(0, steeringCurve.Evaluate(speed), 0);
                tire.transform.localRotation = rotation;
            }
        }
        else
        {
            foreach (Tire tire in car.frontTires)
            {
                Quaternion rotation = Quaternion.Euler(0, 0, 0);
                tire.transform.localRotation = rotation;
            }
        }
    }

    private void HandleGrip()
    {
        foreach (Tire tire in tires)
        {
            Transform rayPoint = tire.transform;

            RaycastHit hit;
            float maxLength = suspension.restLength + suspension.springTravel;

            // check if the car is not on the ground
            if (!Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + tire.radius, car.driveableLayer))
            {
                Debug.Log("car is not on the ground");
                return;
            }

            Vector3 steeringDirection = rayPoint.right;
            Vector3 tireWorldVelocity = car.rigidBody.GetPointVelocity(rayPoint.position);

            float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVelocity);

            float desiredVelocityChange = -steeringVelocity * GetCurrentTireGrip(tire);

            float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

            Vector3 netForce = steeringDirection * desiredAcceleration * tire.mass;
            car.rigidBody.AddForceAtPosition(netForce, rayPoint.position);
        }
    }

    private float GetCurrentTireGrip(Tire tire)
    {
        return tire.gripCurve.Evaluate(powertrain.GetCurrentSpeed());
    }
}
