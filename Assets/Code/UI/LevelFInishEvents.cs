using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelFinishEvents : MonoBehaviour
{
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

    private void OnEnable() => StartCoroutine(EnableButtons());

    private void OnDisable()
    {
        nextLevelButton.clicked -= GameManager.Instance.StartNextLevel;
        retryButton.clicked -= GameManager.Instance.RestartLevel;
    }

    private IEnumerator EnableButtons()
    {
        while (GameManager.Instance == null || InputManager.Instance == null)
            yield return new WaitForFixedUpdate();

        nextLevelButton.clicked += GameManager.Instance.StartNextLevel;
        retryButton.clicked += GameManager.Instance.RestartLevel;
    }
}
