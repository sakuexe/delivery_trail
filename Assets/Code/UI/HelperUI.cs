using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public enum HelperType
{
    Respawn,
    Accelerate,
}

public class HelperUI : MonoBehaviour
{
    [SerializeField]
    private bool showHelpers = true;
    [SerializeField]
    private float helperThreshold = 5f;

    // UI elements
    private UIDocument helperDocument;
    private VisualElement baseContainer;
    private VisualElement respawnHelper;

    // states
    private float _playerStoppedAt;
    private bool _gameHasStarted;
    private bool _levelHasStarted;
    private bool _playerHasStarted;

    void Awake()
    {
        helperDocument = gameObject.GetComponent<UIDocument>();
        respawnHelper = helperDocument.rootVisualElement.Q("RespawnHelper") as VisualElement;
    }

    void Start()
    {
        if (!showHelpers)
            return;
        GameManager.Instance.onLevelStarted += StartHelpCheck;
        GameManager.Instance.onPlayerRespawn += Respawn;
    }

    void OnDisable()
    {
        GameManager.Instance.onLevelStarted -= StartHelpCheck;
        GameManager.Instance.onPlayerRespawn -= Respawn;
    }

    private void StartHelpCheck()
    {
        if (_levelHasStarted)
            return;
        _levelHasStarted = true;
        StartCoroutine(CheckForHelp());
    }

    private IEnumerator CheckForHelp()
    {
        while (true)
        {
            if (CanShowRespawnHelper())
                ShowHelper(respawnHelper);
            else
                HideHelper(respawnHelper);


            yield return new WaitForSeconds(1);
        }
    }

    private bool CanShowRespawnHelper()
    {
        float playerSpeed = GameManager.Instance.player.powertrain.GetCurrentSpeed();

        if (playerSpeed > 10 && !_playerHasStarted)
            _playerHasStarted = true;
        if (playerSpeed > 1 && _playerStoppedAt > 0)
            _playerStoppedAt = 0;

        if (_playerHasStarted && playerSpeed < 1 && _playerStoppedAt == 0)
            _playerStoppedAt = Time.time;

        if (_playerStoppedAt == 0)
            return false;
        if (Time.time - _playerStoppedAt < helperThreshold)
            return false;

        return true;
    }

    private void ShowHelper(VisualElement helperElement) => helperElement.RemoveFromClassList("hidden");

    private void HideHelper(VisualElement helperElement) => helperElement.AddToClassList("hidden");

    public void Respawn()
    {
        _playerStoppedAt = 0;
        _playerHasStarted = false;
        HideHelper(respawnHelper);
    }
}
