using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuEvents : MonoBehaviour
{
    [SerializeField]
    private string firstLevelScene = "Level 1";
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip hoverSoundEffect;
    [SerializeField]
    private AudioClip clickSoundEffect;

    private Button startButton;
    private Button settingsButton;
    private Button exitButton;

    private Button[] buttons;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        startButton = root.Q<Button>("startbutton");
        settingsButton = root.Q<Button>("settingsbutton");
        exitButton = root.Q<Button>("exitbutton");

        buttons = new Button[] { startButton, settingsButton, exitButton };
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

        // add the sound effects
        foreach (Button button in buttons)
        {
            button.RegisterCallback<PointerEnterEvent>(OnButtonHover);
            button.RegisterCallback<FocusInEvent>(OnButtonFocus);
            button.clicked += OnButtonPress;
        }
    }

    // remember to remove the events when the scene is no longer in use
    void OnDisable()
    {
        startButton.clicked -= StartGame;
        settingsButton.clicked -= OpenSettings;
        foreach (Button button in buttons)
            button.clicked -= OnButtonPress;
    }

    private void StartGame() => SceneManager.LoadScene(firstLevelScene);

    private void OpenSettings()
    {
        Debug.Log("Settings menu opened!");
        // Lisää toiminnallisuus asetusvalikon avaamiseen
    }

    void OnButtonHover(PointerEnterEvent evt) => audioSource.PlayOneShot(hoverSoundEffect);

    void OnButtonFocus(FocusInEvent evt) => audioSource.PlayOneShot(hoverSoundEffect);

    void OnButtonPress() => audioSource.PlayOneShot(clickSoundEffect);
}
