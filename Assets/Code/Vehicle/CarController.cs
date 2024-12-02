using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Braking))]
[RequireComponent(typeof(Steering))]
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
    public Tire[] frontTires;
    public Tire[] rearTires;

    [Header("Layer mask")]
    public LayerMask driveableLayer;

    [Header("Respawn")]
    [SerializeField]
    [Range(0.1f, 5)]
    private float respawnCooldown = 0.5f;

    // references to other components
    // doesn't show in the editor
    public Braking braking { get; private set; }
    public Steering steering { get; private set; }
    public Powertrain powertrain { get; private set; }
    public Rigidbody rigidBody { get; private set; }

    // states
    // powertrain
    private float _gasPedalAmount;
    // steering
    private Vector2 _steeringAmount;
    // braking
    private float _brakePedalAmount;
    // reversing
    public bool isReversing { get; private set; }
    // respawn cooldown
    private float lastRespawnPress;
    private bool canRespawn => Time.time - lastRespawnPress > respawnCooldown;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        powertrain = GetComponent<Powertrain>();
        steering = GetComponent<Steering>();
        braking = GetComponent<Braking>();
    }

    void Start()
    {
        // turn off the motor when the start countdown has not started
        powertrain.enabled = false;
        GameManager.Instance.onLevelStarted += () => powertrain.enabled = true;
    }

    void OnDisable() => GameManager.Instance.onLevelStarted -= () => powertrain.enabled = true;

    // use fixed update, because these things are tied to the physics
    void FixedUpdate()
    {
        // control powertrain
        if (powertrain.enabled)
            powertrain.HandleRpm(_gasPedalAmount);

        if (_gasPedalAmount > 0 && powertrain.enabled)
            powertrain.Accelerate(_gasPedalAmount);

        // control steering
        steering.SteerTires(_steeringAmount);

        // control braking
        if (_brakePedalAmount > 0 && !isReversing)
            braking.Brake(_brakePedalAmount);

        // control reversing
        if (_brakePedalAmount > 0 && CanReverse())
            braking.Reverse();

        // HUD
        UpdatePowertrainHUD();
    }

    /// <summary>
    /// Check if the car can use the reverse gear
    /// </summary>
    /// <returns>true or false</returns>
    private bool CanReverse()
    {
        if (_brakePedalAmount <= 0) return false;
        if (powertrain.GetCurrentSpeed() > braking.maxReversingSpeed) return false;

        Vector3 worldVelocity = rigidBody.GetPointVelocity(transform.position);
        float forwardVelocity = Vector3.Dot(transform.forward, worldVelocity);

        if (forwardVelocity > 0) return false;

        isReversing = true;
        return true;
    }

    /// <summary>
    /// Updates the RPM and Speed values in the HUD to match the values of the powertrain.
    /// </summary>
    private void UpdatePowertrainHUD()
    {
        if (showRpmUI)
            HUDManager.Instance.UpdateRPM(powertrain.GetCurrentRpm());
        if (showSpeedUI)
            HUDManager.Instance.UpdateSpeed(powertrain.GetCurrentSpeed());
    }

    /// <summary>
    /// Handle respawning ot the latest checkpont and applying the rigidbody state of
    /// when the player first acitvated the checkpoint.
    /// </summary>
    private void Respawn()
    {
        // add a cooldown for respawning
        if (!canRespawn) return;
        lastRespawnPress = Time.time;

        if (GameManager.Instance.checkpoints.Count == 0)
        {
            Debug.LogError("No checkpoints found in GameManager.checkpoints (length is 0)");
            return;
        }

        // if the there is only one checkpoint, also restart the level timer
        // this way it is easy to restart runs
        if (GameManager.Instance.checkpoints.Count == 1)
            GameManager.Instance.onLevelStarted.Invoke();

        GameManager.Instance.onPlayerRespawn.Invoke();

        Checkpoint lastCheckpoint = GameManager.Instance.checkpoints.LastOrDefault();

        StartCoroutine(lastCheckpoint.ApplyToRigidbody(rigidBody));
        powertrain.SetCurrentRpm(lastCheckpoint.rpm);
    }

    /// <summary>
    /// Waits for the <paramref name="GameManager"/> to not be null before calling
    /// the <paramref name="GameManager.Instance.onControlSchemeChanged"/> event.
    /// </summary>
    private IEnumerator WaitForGameManager(PlayerInput playerInput)
    {
        while (GameManager.Instance == null)
            yield return new WaitForFixedUpdate();

        // call the onControlSchemeChanged event
        GameManager.Instance.onControlSchemeChanged?.Invoke(playerInput.currentControlScheme);
    }

    // when the player presses on the gas
    public void OnGas(InputValue value)
    {
        isReversing = false;
        _gasPedalAmount = value.Get<float>();
    }

    // when the player pressed on the brake
    public void OnBrake(InputValue value)
    {
        _brakePedalAmount = value.Get<float>();
        isReversing = _brakePedalAmount > 0 && CanReverse();
    }

    // when the player steers the car
    public void OnSteering(InputValue value) => _steeringAmount = value.Get<Vector2>();

    public void OnRespawn(InputValue value) => Respawn();

    public void OnControlsChanged(PlayerInput playerInput) => StartCoroutine(WaitForGameManager(playerInput));
}
