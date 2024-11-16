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

    [Header("Camera Settings")]
    public Camera mainCamera;
    [SerializeField]
    private float fovSensitivity = 5;
    [SerializeField]
    private float cameraSmoothness = 5f;
    [SerializeField]
    private int maxFov = 110;

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
        baseFov = mainCamera.fieldOfView;
    }

    void Update()
    {
        // make the camera zoom out during high speeds, for better sense of speed
        float fovMultiplier = powertrain.GetCurrentSpeed() * (fovSensitivity / 10);
        float targetFov = baseFov + fovMultiplier;

        // clamp the target fov
        targetFov = Mathf.Clamp(targetFov, 0, maxFov);

        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * cameraSmoothness);
    }

}
