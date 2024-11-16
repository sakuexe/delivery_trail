using UnityEngine;
using UnityEngine.Assertions;


[RequireComponent(typeof(Steering))]
[RequireComponent(typeof(Suspension))]
[RequireComponent(typeof(Powertrain))]
public class CarController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rigidBody;
    public Tire[] frontTires;
    public Tire[] rearTires;
    private Powertrain powertrain;

    [Header("Layer mask")]
    public LayerMask driveableLayer;

    // states
    private float baseFov;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();
        // check that the rigidbody is not null, throw error if it is
        Assert.IsNotNull(rigidBody);
        powertrain = GetComponent<Powertrain>();
    }
}
