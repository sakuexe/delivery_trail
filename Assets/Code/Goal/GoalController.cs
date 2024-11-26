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
    private float _startTime;
    private bool _levelFinished = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!resultDocument)
            throw new ArgumentNullException("No hudDocument given to the Goal Controller (in Goal -object)");
        if (!countdownDocument)
            throw new ArgumentNullException("No countdownDocument given to the Goal Controller (in Goal -object)");
        if (!GameManager.Instance)
            throw new ArgumentNullException(String.Join(
                "No game manager found in scene. Please add an empty gameObject to the scene",
                "and add the GameManager-script to it (Assets/Code/GameManager.cs)"));
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

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60); // Get minutes
        int seconds = Mathf.FloorToInt(time % 60); // Get remaining seconds
        int milliseconds = Mathf.FloorToInt((time * 100) % 100); // Get milliseconds (hundredths of a second)

        // Format the time as MM:SS:ss
        return string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, milliseconds);
    }

    // this could be a coroutine
    private void SetupMedalTimes()
    {
        Label bronzeTimeLabel = resultDocument.rootVisualElement.Q("BronzeTime_value") as Label;
        Label silverTimeLabel = resultDocument.rootVisualElement.Q("SilverTime_value") as Label;
        Label goldTimeLabel = resultDocument.rootVisualElement.Q("GoldTime_value") as Label;

        bronzeTimeLabel.text = FormatTime(bronzeTime);
        silverTimeLabel.text = FormatTime(silverTime);
        goldTimeLabel.text = FormatTime(goldTime);
    }

    private void ShowGoalScreen()
    {
        float timeTaken = Time.time - _startTime;
        timeTakenLabel.text = FormatTime(timeTaken);
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
