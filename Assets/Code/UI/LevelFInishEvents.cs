using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LevelFinishEvents : MonoBehaviour
{
    [SerializeField]
    private string nextLevelSceneName = "SampleScene";
    private UIDocument resultDocument;
    private Button nextLevelButton;
    private Button retryButton;
    private Button exitButton;

    private void Awake()
    {
        resultDocument = GetComponent<GoalController>().resultDocument;
        nextLevelButton = resultDocument.rootVisualElement.Q("NextButton") as Button;
        retryButton = resultDocument.rootVisualElement.Q("RetryButton") as Button;
        exitButton = resultDocument.rootVisualElement.Q("ExitButton") as Button;
    }

    private void OnEnable()
    {
        nextLevelButton.RegisterCallbackOnce<ClickEvent>(OnNextClick);
        retryButton.RegisterCallbackOnce<ClickEvent>(OnRetryClick);
    }

    private void OnNextClick(ClickEvent evt)
    {
        SceneManager.LoadSceneAsync(nextLevelSceneName);
    }

    private void OnRetryClick(ClickEvent evt)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadSceneAsync(currentScene.name);
    }
}
