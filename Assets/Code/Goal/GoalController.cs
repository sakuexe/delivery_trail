using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GoalController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private UIDocument resultDocument;
    [SerializeField]
    private UIDocument countdownDocument;

    [Header("Functionality")]
    [SerializeField]
    private string playerTag = "Player";

    [Header("Medal Times")]
    [SerializeField]
    private float goldTime;
    [SerializeField]
    private float silverTime;
    [SerializeField]
    private float bronzeTime;

    // ui elements
    private VisualElement mainContainer;
    private GroupBox levelDetails;
    private GroupBox medalDetails;
    private Label timeTakenLabel;
    private Label countdownValue;

    // states
    public float _startTime { get; private set; }
    private bool _levelFinished = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!GameManager.Instance)
        {
            Debug.Log("Please add an empty gameObject to the scene and add the GameManager-script to it (Assets/Code/GameManager.cs)");
            throw new Exception("GameManager not found");
        }
        _startTime = Time.time;

        // fetch the ui elements
        timeTakenLabel = resultDocument.rootVisualElement.Q("Time_value") as Label;
        mainContainer = resultDocument.rootVisualElement.Q("Background") as VisualElement;
        levelDetails = resultDocument.rootVisualElement.Q("LevelDetails") as GroupBox;
        medalDetails = resultDocument.rootVisualElement.Q("MedalTimes") as GroupBox;

        countdownValue = countdownDocument.rootVisualElement.Q("Countdown") as Label;

        mainContainer.style.opacity = 0;
        levelDetails.AddToClassList("hidden");
        medalDetails.AddToClassList("hidden");

        SetupMedalTimes();

        // set up the countdown timer
        GameManager.Instance.onStartCountdownChanged += UpdateStartCountdown;
        GameManager.Instance.onLevelStarted += HideStartCountdown;
        UpdateStartCountdown(GameManager.Instance.startCountdownTime);
    }

    private void OnDisable()
    {
        GameManager.Instance.onStartCountdownChanged -= UpdateStartCountdown;
        GameManager.Instance.onLevelStarted -= () => resultDocument.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // don't let the player re-finish the level
        if (_levelFinished)
            return;
        // ignore everything that does not have the playerTag
        if (!other.transform.parent.gameObject)
            return;
        if (other.transform.parent.gameObject.tag != playerTag)
            return;

        _levelFinished = true;
        ShowGoalScreen();
    }

    // this could be a coroutine
    private void SetupMedalTimes()
    {
        Label bronzeTimeLabel = resultDocument.rootVisualElement.Q("BronzeTime_value") as Label;
        Label silverTimeLabel = resultDocument.rootVisualElement.Q("SilverTime_value") as Label;
        Label goldTimeLabel = resultDocument.rootVisualElement.Q("GoldTime_value") as Label;

        bronzeTimeLabel.text = GameManager.Instance.FormatTime(bronzeTime);
        silverTimeLabel.text = GameManager.Instance.FormatTime(silverTime);
        goldTimeLabel.text = GameManager.Instance.FormatTime(goldTime);
    }

    private void ShowGoalScreen()
    {
        float timeTaken = Time.time - _startTime;
        timeTakenLabel.text = GameManager.Instance.FormatTime(Time.time - GameManager.Instance.startTime); 
        mainContainer.style.opacity = 1f;
        levelDetails.RemoveFromClassList("hidden");
        medalDetails.RemoveFromClassList("hidden");
    }

    private void UpdateStartCountdown(int value)
    {
        if (value <= 0)
        {
            countdownValue.text = $"GO!";
            return;
        }
        countdownValue.text = $"{value}";
    }

    private void HideStartCountdown()
    {
        countdownValue.AddToClassList("hidden");
    }
}
