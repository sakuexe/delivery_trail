using UnityEngine;

[RequireComponent(typeof(Braking))]
[RequireComponent(typeof(Steering))]
[RequireComponent(typeof(Suspension))]
[RequireComponent(typeof(Powertrain))]
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Visualization")]
    [SerializeField]
    private bool showRpmUI = true;
    [SerializeField]
    private bool showSpeedUI = true;

    [Header("References")]
    public Rigidbody rigidBody { get; private set; }
    public Tire[] frontTires;
    public Tire[] rearTires;

    // references to other components
    // doesn't show in the editor
    public Braking braking { get; private set; }
    public Steering steering { get; private set; }
    public Suspension suspension { get; private set; }
    public Powertrain powertrain { get; private set; }

    [Header("Layer mask")]
    public LayerMask driveableLayer;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        powertrain = GetComponent<Powertrain>();
    }

    void Start()
    {
        // turn off the motor when the start countdown has not started
        powertrain.enabled = false;
        GameManager.Instance.onLevelStarted += () => powertrain.enabled = true;
    }

    void OnDisable()
    {
        GameManager.Instance.onLevelStarted -= () => powertrain.enabled = true;
    }

    void FixedUpdate()
    {
        UpdatePowerTrainUI();
    }

    private void UpdatePowerTrainUI()
    {
        if (showRpmUI)
            UIManager.Instance.UpdateRPM(powertrain.GetCurrentRpm());
        if (showSpeedUI)
            UIManager.Instance.UpdateSpeed(powertrain.GetCurrentSpeed());
    }
}
