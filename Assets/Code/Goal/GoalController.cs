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

    // ui elements
    private VisualElement mainContainer;
    private VisualElement reviewStars;
    private GroupBox medalDetails;
    private Label timeTakenLabel;
    private Label countdownValue;

    // states
    private bool _levelFinished = false;

    private void OnEnable()
    {
        // fetch the ui elements
        timeTakenLabel = resultDocument.rootVisualElement.Q("Time_value") as Label;
        mainContainer = resultDocument.rootVisualElement.Q("Background") as VisualElement;
        medalDetails = resultDocument.rootVisualElement.Q("MedalTimes") as GroupBox;
        reviewStars = resultDocument.rootVisualElement.Q("ReviewStars") as VisualElement;

        countdownValue = countdownDocument.rootVisualElement.Q("Countdown") as Label;

        mainContainer.style.opacity = 0;
        mainContainer.SetEnabled(false);

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
        mainContainer.SetEnabled(true);
        float timeTaken = Time.time - GameManager.Instance.startTime;
        UpdateReview(timeTaken);

        timeTakenLabel.text = GameManager.Instance.FormatTime(timeTaken); 
        mainContainer.style.opacity = 1f;
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
        List<VisualElement> stars = reviewStars.Query<VisualElement>(className:"star-stroke").ToList();
        ReviewGenerator.UpdateStars(stars, starsAchieved);

        // get the customer data
        Review review = ReviewGenerator.GenerateReview(starsAchieved);
        mainContainer.Q<Label>("CustomerName").text = review.name;
        mainContainer.Q<Label>("CustomerReview").text = review.review;
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
