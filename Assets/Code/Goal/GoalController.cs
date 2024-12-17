using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GoalController : MonoBehaviour
{
    [Header("UI")]
    public UIDocument resultDocument;
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

    private BoxCollider goalTrigger;

    // ui elements
    private VisualElement mainContainer;
    private VisualElement reviewStars;
    private GroupBox medalDetails;
    private Label timeTakenLabel;
    private Label countdownValue;
    private VisualElement finishContainer;
    private Label customerName;
    private Label customerReview;

    // states
    private bool _levelFinished = false;

    private void Start()
    {
        // disable the goal trigger by default
        goalTrigger = GetComponent<BoxCollider>();
        goalTrigger.enabled = false;
    }

    private void OnEnable()
    {
        // fetch the ui elements
        timeTakenLabel = resultDocument.rootVisualElement.Q("Time_value") as Label;
        mainContainer = resultDocument.rootVisualElement.Q("Background") as VisualElement;
        medalDetails = resultDocument.rootVisualElement.Q("MedalTimes") as GroupBox;
        reviewStars = resultDocument.rootVisualElement.Q("ReviewStars") as VisualElement;

        countdownValue = countdownDocument.rootVisualElement.Q("Countdown") as Label;
        finishContainer = mainContainer.Q<VisualElement>("FinishContainer");
        customerName = mainContainer.Q<Label>("CustomerName");
        customerReview = mainContainer.Q<Label>("CustomerReview");

        /*finishContainer.AddToClassList("hidden");*/
        mainContainer.style.opacity = 0;
        resultDocument.rootVisualElement.SetEnabled(false);

        SetupMedalTimes();

        // set up the countdown timer
        GameManager.Instance.onStartCountdownChanged += UpdateStartCountdown;
        GameManager.Instance.onLevelStarted += HideStartCountdown;
        UpdateStartCountdown(GameManager.Instance.startCountdownTime);
        GameManager.Instance.onAllCheckpointsCleared += EnableGoal;
    }

    private void OnDisable()
    {
        GameManager.Instance.onStartCountdownChanged -= UpdateStartCountdown;
        GameManager.Instance.onLevelStarted -= () => resultDocument.enabled = true;
        GameManager.Instance.onAllCheckpointsCleared -= EnableGoal;
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
        GameManager.Instance.onLevelFinished.Invoke();
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
        resultDocument.rootVisualElement.SetEnabled(true);
        mainContainer.style.opacity = 1f;
        float timeTaken = Time.time - GameManager.Instance.startTime;
        UpdateReview(timeTaken);

        timeTakenLabel.text = GameManager.Instance.FormatTime(timeTaken); 
        finishContainer.RemoveFromClassList("hidden");
        medalDetails.RemoveFromClassList("hidden");
    }

    private void UpdateReview(float timeTaken)
    {
        int starsAchieved = 0;
        if (timeTaken <= goldTime)
            starsAchieved = 3;
        else if (timeTaken <= silverTime)
            starsAchieved = 2;
        else if (timeTaken <= bronzeTime)
            starsAchieved = 1;
        List<VisualElement> stars = reviewStars.Query<VisualElement>(className: "star-stroke").ToList();
        ReviewGenerator.UpdateStars(stars, starsAchieved);

        // get the customer data
        Review review = ReviewGenerator.GenerateReview(starsAchieved);
        customerName.text = review.name;
        customerReview.text = review.review;
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

    private void EnableGoal()
    {
        // TODO: add visual clarity for if the goal is ready to be reached
        goalTrigger.enabled = true;
    }
}
