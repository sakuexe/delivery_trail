using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    // the UIManager is a singleton, since we don't want multiple of them
    // you can call it with UIManager.Instance.<so-on> from wherever you want
    public static UIManager Instance { get; private set; }
    private UIDocument hudDocument;
    private Label rpmValue;
    private Label speedValue;
    private Label levelTimer;


    // gets run before the start function
    void Awake()
    {
        // making the singleton work
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        hudDocument = gameObject.GetComponent<UIDocument>();

        // fetch the ui elements
        rpmValue = hudDocument.rootVisualElement.Q("RPM_value") as Label;
        speedValue = hudDocument.rootVisualElement.Q("Speed_value") as Label;
        levelTimer = hudDocument.rootVisualElement.Q("LevelTime") as Label;
    }

    private void Start() => StartCoroutine(UpdateTimer(0.1f));

    /// <summary>
    /// Coroutine for updating the timer every n seconds
    /// </summary>
    private IEnumerator UpdateTimer(float delay)
    {
        while (true)
        {
            if (GameManager.Instance.startTime != 0)
                levelTimer.text = $"{GameManager.Instance.FormatTime(Time.time - GameManager.Instance.startTime)}";
            yield return new WaitForSeconds(delay);
        }
    }

    public void UpdateSpeed(float speed)
    {
        speedValue.text = $"{speed.ToString("0")}";
    }

    public void UpdateRPM(float rpm)
    {
        rpmValue.text = $"{rpm.ToString("0")}";
    }
}
