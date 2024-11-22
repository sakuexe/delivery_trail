using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Suspension))]
[RequireComponent(typeof(Powertrain))]
public class Steering : MonoBehaviour
{
    [SerializeField]
    public AnimationCurve steeringCurve = new(new Keyframe(0, 45), new Keyframe(150, 12));
    [SerializeField]
    [Range(1, 90)]
    private float maxSteeringAngle = 45;
    [SerializeField]
    [Range(1, 30)]
    private float sidewaysGripMultiplier = 10f;

    // references
    private Powertrain powertrain;
    private CarController car;
    private Suspension suspension;
    private Tire[] tires;
    private Transform[] tireModels;

    // states
    private Vector2 _steeringAngle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();
        suspension = gameObject.GetComponent<Suspension>();
        powertrain = gameObject.GetComponent<Powertrain>();
        tires = car.frontTires.Concat(car.rearTires).ToArray();

        Transform tiresContainer = transform.Find("Model/Tires");
        HashSet<Transform> transforms = new(tiresContainer.GetComponentsInChildren<Transform>());
        transforms.Remove(tiresContainer.transform);
        tireModels = transforms.ToArray();
    }

    // Update is called once per frame
    void FixedUpdate()
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
        foreach (var (tire, index) in car.frontTires.Select((v, i) => (v, i)))
        {
            // get the amount that the tires should be rotating
            float steeringAmount = steeringCurve.Evaluate(speed / 120) * _steeringAngle.x;
            // convert the degrees to a quaternion
            Quaternion targetRotation = Quaternion.Euler(0, steeringAmount * maxSteeringAngle, 0);
            Quaternion currentRotation = Quaternion.Slerp(tire.transform.localRotation, targetRotation, 4f * Time.fixedDeltaTime);

            // and finally add the rotation to the tires
            tire.transform.localRotation = currentRotation;
            // also add the rotation to the models
            tireModels[index].localRotation = currentRotation;
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

            // don't apply if the wheel is not on the ground
            if (!Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + tire.radius, car.driveableLayer))
                continue;

            Vector3 steeringDirection = rayPoint.right;
            Vector3 tireWorldVelocity = car.rigidBody.GetPointVelocity(rayPoint.position);

            float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVelocity);

            float desiredVelocityChange = -steeringVelocity * tire.GetCurrentTireGrip(powertrain.GetCurrentSpeed());

            float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

            Vector3 netForce = steeringDirection * desiredAcceleration * tire.mass;
            car.rigidBody.AddForceAtPosition(netForce * sidewaysGripMultiplier, rayPoint.position);
        }
    }
}
