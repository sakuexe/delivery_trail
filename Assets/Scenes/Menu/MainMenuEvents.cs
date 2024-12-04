using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuEvents : MonoBehaviour
{
    [SerializeField]
    private string firstLevelScene = "Level 1";

    private Button startButton;
    private Button settingsButton;
    private Button exitButton;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        startButton = root.Q<Button>("startbutton");
        settingsButton = root.Q<Button>("settingsbutton");
        exitButton = root.Q<Button>("exitbutton");
    }

    // use OnEnable for event registering
    void OnEnable()
    {
        if (startButton == null || settingsButton == null || exitButton == null)
        {
            Debug.Log($"startButton: {startButton != null}, settingsButton: {settingsButton != null}, exitButton: {exitButton != null}");
            Debug.LogError("All of the main menu buttons were not found");
            return;
        }

        startButton.clicked += StartGame;
        settingsButton.clicked += OpenSettings;
        // exit button event can be an anonymus function since it stops the game anyways
        exitButton.clicked += () => Application.Quit();
    }

    // remember to remove the events when the scene is no longer in use
    void OnDisable()
    {
        startButton.clicked -= StartGame;
        settingsButton.clicked -= OpenSettings;
    }

    private void StartGame() => SceneManager.LoadScene(firstLevelScene);

    private void OpenSettings()
    {
        Debug.Log("Settings menu opened!");
        // Lis‰‰ toiminnallisuus asetusvalikon avaamiseen
    }
}
