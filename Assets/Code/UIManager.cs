using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    // the UIManager is a singleton, since we don't want multiple of them
    // you can call it with UIManager.Instance.<so-on> from wherever you want
    public static UIManager Instance { get; private set; }
    private UIDocument document;
    private Label rpmValue;
    private Label speedValue;

    // gets run before the start function
    void Awake()
    {
        // making the singleton work
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        // fetch the ui elements
        document = GetComponent<UIDocument>();
        rpmValue = document.rootVisualElement.Q("RPM_value") as Label;
        speedValue = document.rootVisualElement.Q("Speed_value") as Label;
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
