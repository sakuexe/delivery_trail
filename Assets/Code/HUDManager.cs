using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(PlayerInput))]
public class HUDManager : MonoBehaviour
{
    // the HUDManager is a singleton, since we don't want multiple of them
    // you can call it with HUDManager.Instance.<so-on> from wherever you want
    public static HUDManager Instance { get; private set; }

    [SerializeField]
    private UIDocument basicsDocument;
    [SerializeField]
    private UIDocument menuDocument;
    private PlayerInput playerInput;
    public HelperUI helperUI { get; private set; }

    // HUD
    private VisualElement baseContainer;
    private VisualElement menuContainer;
    private Label rpmValue;
    private Label speedValue;
    private Label levelTimer;

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
        playerInput = GetComponent<PlayerInput>();

        // fetch the ui elements
        baseContainer = basicsDocument.rootVisualElement.Q("Base") as VisualElement;
        rpmValue = basicsDocument.rootVisualElement.Q("RPM_value") as Label;
        speedValue = basicsDocument.rootVisualElement.Q("Speed_value") as Label;
        levelTimer = basicsDocument.rootVisualElement.Q("LevelTime") as Label;
        menuContainer = menuDocument.rootVisualElement.Q<VisualElement>("EscapeMenuContainer");
    }

    void Start()
    {
        StartCoroutine(UpdateTimer());
        // stop the timer once the level finishes
        GameManager.Instance.onLevelFinished += () => HideHud();
        menuContainer.SetEnabled(false);
    }

    void OnDisable() => GameManager.Instance.onLevelFinished -= () => HideHud();

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
    private void HideHud()
    {
        baseContainer.style.opacity = 0;
    }

    public void UpdateSpeed(float speed)
    {
        speedValue.text = $"{speed.ToString("0")}";
    }

    public void UpdateRPM(float rpm)
    {
        rpmValue.text = $"{rpm.ToString("0")}";
    }

    /// <summary>
    /// Toggle the visibility of the "escape" menu.
    /// </summary>
    public void OnCancel()
    {
        Debug.Log("cancel is pressed");
        if (menuContainer.enabledSelf)
        {
            menuContainer.SetEnabled(false);
            menuContainer.style.opacity = 0f;
        }
        else {
            menuContainer.style.opacity = 1f;
            menuContainer.SetEnabled(true);
        }
    }
}
