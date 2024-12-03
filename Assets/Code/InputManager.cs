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

    // player inputs
    public Action<float> onAccelerator;
    public Action<float> onBrake;
    public Action<Vector2> onSteering;
    public Action<string> onControlSchemeChanged;
    public Action onRespawn;
    public Action onPause;
    // UI inputs

    void Awake()
    {
        // making the singleton work
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private IEnumerator InvokeControlSchemeChange(PlayerInput playerInput)
    {
        // make sure that the HUDManager is ready (if not, the controller icon is not set at start)
        while (HUDManager.Instance == null)
            yield return new WaitForFixedUpdate();

        onControlSchemeChanged?.Invoke(playerInput.currentControlScheme);
    }

    public void OnGas(InputValue value) => onAccelerator?.Invoke(value.Get<float>());

    public void OnBrake(InputValue value) => onBrake?.Invoke(value.Get<float>());

    public void OnSteering(InputValue value) => onSteering?.Invoke(value.Get<Vector2>());

    public void OnRespawn(InputValue value) => onRespawn?.Invoke();

    public void OnPause(InputValue value) => onPause?.Invoke();

    public void OnControlsChanged(PlayerInput playerInput) => StartCoroutine(InvokeControlSchemeChange(playerInput));
}
