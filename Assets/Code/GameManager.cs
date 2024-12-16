using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CarController player { get; private set; }
    [SerializeField]
    private string nextLevelScene = "SampleScene";
    [SerializeField]
    private string mainMenuScene = "MainMenu";
    // events for game state, that other components can listen for
    // usage: GameManager.Instance.onLevelStarted += YourCustomFunction;
    public Action onLevelStarted;
    public Action onLevelFinished;
    public Action onPlayerRespawn;
    public Action<int> onStartCountdownChanged;
    public Action<string> onControlSchemeChanged;

    public int startCountdownTime { get; private set; } = 3;
    public float startTime { get; private set; }
    public List<Checkpoint> checkpoints = new();

    private int _startCountdown;

    void Awake()
    {
        // making the singleton work
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void Start()
    {
        StartCoroutine(StartCountdown());
        onLevelStarted += StartLevelTimer;
        player = GameObject.FindWithTag("Player").GetComponent<CarController>();
        Checkpoint initialCheckpoint = new (player.rigidBody);
        checkpoints.Add(initialCheckpoint);

        // enable vsync
        QualitySettings.vSyncCount = 1;
    }

    private void OnDisable()
    {
        onLevelStarted -= StartLevelTimer;
    }

    /// <summary>
    /// Coroutine for counting down the level start.
    /// </summary>
    private IEnumerator StartCountdown()
    {
        _startCountdown = startCountdownTime;

        // wait for n amount of seconds
        while (_startCountdown > 0)
        {
            yield return new WaitForSeconds(1);
            _startCountdown--;
            onStartCountdownChanged?.Invoke(_startCountdown);
        }

        Debug.Log("GAME STARTED");
        // invoke the event of game starting
        onLevelStarted?.Invoke();
    }

    private void StartLevelTimer() => startTime = Time.time;

    /// <summary>
    /// Converts time in seconds to 00:00:00 format
    /// </summary>
    /// <param name="time">Time in seconds</param>
    /// <returns>a formatted string, eg. "00:02:50"</returns>
    public string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60); // Get minutes
        int seconds = Mathf.FloorToInt(time % 60); // Get remaining seconds
        int milliseconds = Mathf.FloorToInt((time * 100) % 100); // Get milliseconds (hundredths of a second)

        // Format the time as MM:SS:ss
        return string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, milliseconds);
    }

    /// <summary>
    /// Open the main menu.
    /// </summary>
    public void StartMainMenu() => SceneManager.LoadSceneAsync(mainMenuScene);

    /// <summary>
    /// Change the current level to the next scene that is defined
    /// in the nextLevelScene variable.
    /// </summary>
    public void StartNextLevel() => SceneManager.LoadSceneAsync(nextLevelScene);

    /// <summary>
    /// Restart the current scene.
    /// </summary>
    public void RestartLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadSceneAsync(currentScene.name);
    }
}
