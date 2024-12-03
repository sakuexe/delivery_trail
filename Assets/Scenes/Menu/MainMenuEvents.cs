using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuEvents : MonoBehaviour
{
    private void OnEnable()
    {     
        var root = GetComponent<UIDocument>().rootVisualElement;
     
        var startButton = root.Q<Button>("startbutton");
        var settingsButton = root.Q<Button>("settingsbutton");
        var exitButton = root.Q<Button>("exitbutton");

        if (startButton != null)
            startButton.clicked += () => SceneManager.LoadScene("Level 1");

        if (settingsButton != null)
            settingsButton.clicked += OpenSettings;

        if (exitButton != null)
            exitButton.clicked += () => Application.Quit();
    }

    private void OpenSettings()
    {
        Debug.Log("Settings menu opened!");
        // Lis‰‰ toiminnallisuus asetusvalikon avaamiseen
    }
}