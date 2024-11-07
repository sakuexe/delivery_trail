using UnityEngine;
using UnityEngine.Assertions;

public class CarScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Rigidbody carRb;
    [SerializeField]
    private Transform[] rayPoints;
    [SerializeField]
    private LayerMask driveable;

    [Header("Suspension Settings")]
    [SerializeField]
    private float springStiffness;
    [SerializeField]
    private float damperStiffness;
    [SerializeField]
    private float restLength;
    [SerializeField]
    private float springTravel;
    [SerializeField]
    private float wheelRadius;

    [Header("Speed Settings")]
    [SerializeField]
    private float speed = 100;
    [SerializeField]
    private float steeringSpeed = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (carRb == null)
            carRb = GetComponent<Rigidbody>();
        // check that the rigidbody is not null, throw error if it is
        Assert.IsNotNull(carRb);
    }

    void FixedUpdate()
    {
        Suspension();
        Control();
    }

    private void Control()
    {
        if (Input.GetKey(KeyCode.W))
        {
            carRb.AddRelativeForce(Vector3.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            carRb.AddRelativeForce(Vector3.left * steeringSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            carRb.AddRelativeForce(Vector3.right * steeringSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            carRb.AddRelativeForce(Vector3.back * speed * Time.deltaTime);
        }
    }

    private void Suspension()
    {
        foreach (Transform rayPoint in rayPoints)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + wheelRadius, driveable))
            {
                Debug.Log("hit!");
                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = restLength - currentSpringLength / springTravel;

                float springVelocity = Vector3.Dot(carRb.GetPointVelocity(rayPoint.position), rayPoint.up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                carRb.AddForceAtPosition(netForce * rayPoint.up, rayPoint.position);

                Debug.DrawLine(rayPoint.position, hit.point, Color.red);
            }
            else
            {
                Debug.Log("not hitting!");
                Debug.DrawLine(rayPoint.position, rayPoint.position + (wheelRadius + maxLength) * -rayPoint.up, Color.green);
            }
        }
    }
}
