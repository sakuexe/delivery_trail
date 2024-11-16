using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Powertrain))]
[RequireComponent(typeof(Suspension))]
[RequireComponent(typeof(CarController))]
public class Braking : MonoBehaviour
{
    [SerializeField]
    [Range(2_000, 10_000)]
    private float breakForce = 5_000;

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

    private void HandleBraking()
    {
        Vector3 worldVelocity = car.rigidBody.GetPointVelocity(car.transform.position);
        float forwardVelocity = Vector3.Dot(car.transform.forward, worldVelocity);

        Vector3 decelerationDirection = forwardVelocity < 0 ? car.transform.forward : -car.transform.forward;

        float deceleration = (breakForce * _brakePressed) / car.rigidBody.mass;
        float decelarationForce = car.rigidBody.mass * deceleration;

        foreach (Tire tire in car.frontTires)
        {
            if (suspension.IsGrounded(tire) == null)
                continue;
            float tireBrakeForce = (decelarationForce / 2);
            car.rigidBody.AddForceAtPosition(decelerationDirection * decelarationForce, tire.transform.position);
        }
    }
}
