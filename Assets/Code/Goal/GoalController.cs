using UnityEngine;
using UnityEngine.UIElements;

public class GoalController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private UIDocument document;

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

    // states
    private float _startTime;
    private bool _levelFinished = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!document)
            Debug.LogError("No UIDocument given to the Goal Controller (in Goal -object)");
        _startTime = Time.time;

        // fetch the ui elements
        timeTakenLabel = document.rootVisualElement.Q("Time_value") as Label;
        mainContainer = document.rootVisualElement.Q("Background") as VisualElement;
        levelDetails = document.rootVisualElement.Q("LevelDetails") as GroupBox;
        medalDetails = document.rootVisualElement.Q("MedalTimes") as GroupBox;

        mainContainer.AddToClassList("hidden");
        levelDetails.AddToClassList("hidden");
        medalDetails.AddToClassList("hidden");

        SetupMedalTimes();
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
        Label bronzeTimeLabel = document.rootVisualElement.Q("BronzeTime_value") as Label;
        Label silverTimeLabel = document.rootVisualElement.Q("SilverTime_value") as Label;
        Label goldTimeLabel = document.rootVisualElement.Q("GoldTime_value") as Label;

        bronzeTimeLabel.text = FormatTime(bronzeTime);
        silverTimeLabel.text = FormatTime(silverTime);
        goldTimeLabel.text = FormatTime(goldTime);
    }

    private void ShowGoalScreen()
    {
        float timeTaken = Time.time - _startTime;
        timeTakenLabel.text = FormatTime(timeTaken);
        mainContainer.RemoveFromClassList("hidden");
        levelDetails.RemoveFromClassList("hidden");
        medalDetails.RemoveFromClassList("hidden");
    }
}
