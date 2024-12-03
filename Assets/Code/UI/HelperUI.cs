using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum HelperType
{
    Respawn,
    Accelerate,
}

public class HelperUI : MonoBehaviour
{
    [SerializeField]
    private bool showHelpers = true;
    // how long it takes after the condition is matched to show helper
    [SerializeField]
    private float helperThreshold = 5f;
    // how often do we check if there are helpers needed
    [SerializeField]
    private float checkHelpCooldown = 1f;

    // UI elements
    private UIDocument helperDocument;
    private VisualElement baseContainer;
    private VisualElement respawnHelper;

    // states
    private float _playerStoppedAt;
    private bool _gameHasStarted;
    private bool _levelHasStarted;
    private bool _playerHasStarted;

    void Awake()
    {
        helperDocument = gameObject.GetComponent<UIDocument>();
        respawnHelper = helperDocument.rootVisualElement.Q("RespawnHelper") as VisualElement;
    }

    void Start()
    {
        if (!showHelpers)
            return;
        GameManager.Instance.onLevelStarted += StartHelpCheck;
        GameManager.Instance.onPlayerRespawn += Respawn;
        InputManager.Instance.onControlSchemeChanged += ChangeControlIcons;
    }

    void OnDisable()
    {
        GameManager.Instance.onLevelStarted -= StartHelpCheck;
        GameManager.Instance.onPlayerRespawn -= Respawn;
        InputManager.Instance.onControlSchemeChanged -= ChangeControlIcons;
    }

    /// <summary>
    /// Change the icons of all of the helper popups to match with the control scheme.
    /// <summary>
    /// <param name="controlScheme">
    /// <paramref name="PlayerInput.currentControlScheme"/> value
    /// </param>
    /// <see>https://docs.unity3d.com/Packages/com.unity.inputsystem@1.1/api/UnityEngine.InputSystem.PlayerInput.html#UnityEngine_InputSystem_PlayerInput_currentControlScheme</see>
    private void ChangeControlIcons(string controlScheme)
    {
        Debug.Log($"controls changed: {controlScheme}");
        // set the icon class that we want to use
        string wantedIconClass = controlScheme == "Keyboard&Mouse" ? "keyboard" : "controller";

        // get every icon wit the class "helper_icon"
        List<VisualElement> helperIcons = helperDocument.rootVisualElement.Query<VisualElement>(null, "helper-icon").ToList();

        foreach (VisualElement icon in helperIcons)
        {
            // show the ones matching the control scheme
            if (icon.ClassListContains(wantedIconClass))
                icon.style.display = DisplayStyle.Flex;
            // hide all others
            else
                icon.style.display = DisplayStyle.None;
        }
    }

    /// <summary>
    /// Starts checking for available helpers to be shown.
    /// <summary>
    private void StartHelpCheck()
    {
        if (_levelHasStarted)
            return;
        _levelHasStarted = true;
        StartCoroutine(CheckForHelp());
    }

    /// <summary>
    /// Coroutine that checks every <paramref name="checkHelpCooldown"/> if any helpers can be shown.
    /// </summary>
    private IEnumerator CheckForHelp()
    {
        for(;;)
        {
            if (CanShowRespawnHelper())
                ShowHelper(respawnHelper);
            else
                HideHelper(respawnHelper);


            yield return new WaitForSeconds(checkHelpCooldown);
        }
    }

    /// <summary>
    /// A function for validating if a respawn helper can be shown.
    /// </summary>
    private bool CanShowRespawnHelper()
    {
        float playerSpeed = GameManager.Instance.player.powertrain.GetCurrentSpeed();

        if (playerSpeed > 10 && !_playerHasStarted)
            _playerHasStarted = true;
        if (playerSpeed > 1 && _playerStoppedAt > 0)
            _playerStoppedAt = 0;

        if (_playerHasStarted && playerSpeed < 1 && _playerStoppedAt == 0)
            _playerStoppedAt = Time.time;

        if (_playerStoppedAt == 0)
            return false;
        if (Time.time - _playerStoppedAt < helperThreshold)
            return false;

        return true;
    }

    private void ShowHelper(VisualElement helperElement) => helperElement.RemoveFromClassList("hidden");

    private void HideHelper(VisualElement helperElement) => helperElement.AddToClassList("hidden");

    public void Respawn()
    {
        _playerStoppedAt = 0;
        _playerHasStarted = false;
        HideHelper(respawnHelper);
    }
}
