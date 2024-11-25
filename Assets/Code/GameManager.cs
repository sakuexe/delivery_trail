using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // events for game state, that other components can listen for
    // usage: GameManager.Instance.onLevelStarted += YourCustomFunction;
    public Action onLevelStarted;
    public Action<int> onStartCountdownChanged;

    [SerializeField]
    public int startCountdownTime { get; private set; } = 3;
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
    }

    /// <summary>
    /// Coroutine for counting down the level start.
    /// </summary>
    private IEnumerator StartCountdown()
    {
        _startCountdown = startCountdownTime;

        // wait for n amount of seconds
        while (_startCountdown > 0) {
            yield return new WaitForSeconds (1);
            _startCountdown--;
            onStartCountdownChanged?.Invoke(_startCountdown);
        }

        Debug.Log("GAME STARTED");
        // invoke the event of game starting
        onLevelStarted?.Invoke();
    }
}
