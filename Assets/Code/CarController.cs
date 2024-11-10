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

    [Header("Layer mask")]
    public LayerMask driveableLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();
        // check that the rigidbody is not null, throw error if it is
        Assert.IsNotNull(rigidBody);

    }
}
