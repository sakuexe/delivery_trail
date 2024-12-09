using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HUDManager : MonoBehaviour
{
    // the HUDManager is a singleton, since we don't want multiple of them
    // you can call it with HUDManager.Instance.<so-on> from wherever you want
    public static HUDManager Instance { get; private set; }

    [SerializeField]
    private UIDocument basicsDocument;
    [SerializeField]
    private UIDocument menuDocument;
    public HelperUI helperUI { get; private set; }

    // HUD
    private VisualElement baseContainer;
    private VisualElement menuContainer;
    private Label rpmValue;
    private Label speedValue;
    private Label levelTimer;
    private Button retryButton;
    private Button nextLevelButton;
    private Button mainMenuButton;
    private Button exitButton;

    [SerializeField]
    [Range(0.01f, 2)]
    private float timerUpdateDelaySeconds = 0.1f;
    private bool _hasLevelEnded;

    // gets run before the start function
    void Awake()
    {
        // making the singleton work
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        helperUI = GetComponentInChildren<HelperUI>();

        // fetch the HUD elements
        baseContainer = basicsDocument.rootVisualElement.Q("Base") as VisualElement;
        rpmValue = basicsDocument.rootVisualElement.Q("RPM_value") as Label;
        speedValue = basicsDocument.rootVisualElement.Q("Speed_value") as Label;
        levelTimer = basicsDocument.rootVisualElement.Q("LevelTime") as Label;
        menuContainer = menuDocument.rootVisualElement.Q<VisualElement>("EscapeMenuContainer");
        // get the pause menu buttons
        var buttons = menuContainer.Query<Button>().ToList();
        (retryButton) = buttons[0];
        nextLevelButton = buttons[1];
        mainMenuButton = buttons[2];
        exitButton = buttons[3];
    }

    void Start()
    {
        StartCoroutine(UpdateTimer());
        menuContainer.SetEnabled(false);
    }

    private void OnEnable() => StartCoroutine(EnablePauseMenu());

    void OnDisable()
    {
        GameManager.Instance.onLevelFinished -= HideHud;
        InputManager.Instance.onPause -= ToggleMenu;
        retryButton.clicked -= GameManager.Instance.RestartLevel;
        nextLevelButton.clicked -= GameManager.Instance.StartNextLevel;
        mainMenuButton.clicked -= GameManager.Instance.StartMainMenu;
    }

    /// <summary>
    /// Coroutine for enabling the pause menu.
    /// This is done so that we can make sure that the GameManager instance
    /// is ready to use.
    /// </summary>
    private IEnumerator EnablePauseMenu()
    {
        while (GameManager.Instance == null || InputManager.Instance == null)
            yield return new WaitForFixedUpdate();

        GameManager.Instance.onLevelFinished += HideHud;
        InputManager.Instance.onPause += ToggleMenu;
        retryButton.clicked += GameManager.Instance.RestartLevel;
        nextLevelButton.clicked += GameManager.Instance.StartNextLevel;
        mainMenuButton.clicked += GameManager.Instance.StartMainMenu;
        exitButton.clicked += () => Application.Quit();
    }

    /// <summary>
    /// Coroutine for updating the timer every n seconds
    /// </summary>
    private IEnumerator UpdateTimer()
    {
        while (!_hasLevelEnded)
        {
            if (GameManager.Instance.startTime != 0)
                levelTimer.text = $"{GameManager.Instance.FormatTime(Time.time - GameManager.Instance.startTime)}";
            yield return new WaitForSeconds(timerUpdateDelaySeconds);
        }
    }

    /// <summary>
    /// Hides the hud, this can be used when the player finishes the level for example.
    /// Other use case could be when using cinematic camera modes.
    /// </summary>
    public void HideHud() => baseContainer.style.opacity = 0;

    public void UpdateSpeed(float speed) => speedValue.text = $"{speed.ToString("0")}";

    public void UpdateRPM(float rpm) => rpmValue.text = $"{rpm.ToString("0")}";

    /// <summary>
    /// Toggle the visibility of the pause menu.
    /// </summary>
    public void ToggleMenu()
    {
        Debug.Log("HUDManager: toggling menu");
        if (menuContainer.enabledSelf)
        {
            menuContainer.SetEnabled(false);
            menuContainer.style.opacity = 0f;
            InputManager.Instance.playerInput.actions.FindActionMap("UI").Disable();
        }
        else
        {
            menuContainer.style.opacity = 1f;
            menuContainer.SetEnabled(true);
            // change the action map to use UI instead
            // this way the player inputs are disabled
            InputManager.Instance.playerInput.actions.FindActionMap("UI").Enable();
        }
    }
}
