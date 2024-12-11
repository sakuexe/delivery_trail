using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class <c>InputManager</c> handles all the player interaction with the game
/// and fires corresponding c# events and input values when those happen.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public PlayerInput playerInput { get; private set; }

    // player inputs
    public Action<float> onAccelerator;
    public Action<float> onBrake;
    public Action<Vector2> onSteering;
    public Action<string> onControlSchemeChanged;
    public Action onRespawn;
    public Action onPause;

    void Awake()
    {
        // making the singleton work
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        GameManager.Instance.onLevelStarted += LockCursor;
        GameManager.Instance.onLevelFinished += UnlockCursor;
    }

    private void LockCursor() => Cursor.lockState = CursorLockMode.Locked;
    private void UnlockCursor() => Cursor.lockState = CursorLockMode.None;

    /// <summary>
    /// Coroutine for invoking the controlSchemeChange function
    /// at the start of the game.
    /// This is done to make sure that the helper icons are using the right
    /// control scheme's icons.
    /// </summary>
    private IEnumerator InvokeControlSchemeChange(PlayerInput playerInput)
    {
        // make sure that the HUDManager is ready (if not, the controller icon is not set at start)
        while (HUDManager.Instance == null)
            yield return new WaitForFixedUpdate();

        onControlSchemeChanged?.Invoke(playerInput.currentControlScheme);
    }

    // Player inputs
    public void OnGas(InputValue value) => onAccelerator?.Invoke(value.Get<float>());

    public void OnBrake(InputValue value) => onBrake?.Invoke(value.Get<float>());

    public void OnSteering(InputValue value) => onSteering?.Invoke(value.Get<Vector2>());

    public void OnRespawn(InputValue value) => onRespawn?.Invoke();

    public void OnPause(InputValue value)
    {
        onPause?.Invoke();
        UnlockCursor();
    }

    public void OnControlsChanged(PlayerInput playerInput) => StartCoroutine(InvokeControlSchemeChange(playerInput));

    // UI inputs
    /*public void OnCancel(InputValue value)*/
    /*{*/
    /*    Debug.Log("Cancel Pressed");*/
    /*    onPause?.Invoke();*/
    /*    LockCursor();*/
    /*}*/
}
