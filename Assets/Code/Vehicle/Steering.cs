using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private float maxSteeringAngle = 45;

    // states
    private Vector2 _steeringAngle;

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

    // when the player steers the car
    public void OnSteering(InputValue value)
    {
        _steeringAngle = value.Get<Vector2>();
    }

    private void HandleSteering()
    {
        float speed = powertrain.GetCurrentSpeed();
        foreach (Tire tire in car.frontTires)
        {
            // get the amount that the tires should be rotating
            float steeringAmount = steeringCurve.Evaluate(speed) * _steeringAngle.x;
            // convert the degrees to a quaternion
            Quaternion rotation = Quaternion.Euler(0, steeringAmount * maxSteeringAngle, 0);
            // and finally add the rotation to the tires
            tire.transform.localRotation = rotation;
        }
    }

    // handles the tires resistance towards going sideways
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
