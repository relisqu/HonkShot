using Scripts.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState
{
    Play,
    Slowed,
    Pause
}

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    public float TimeSlow = 0.1f;

    private void Awake()
    {
        Instance = this;
        gameState = GameState.Play;
    }

    private GameState gameState;
    private bool _isMenuPaused;

    public bool IsMenuPaused => _isMenuPaused;

    public GameState GameState
    {
        get { return gameState; }
        private set
        {
            gameState = value;

            if (_isMenuPaused)
            {
                Time.timeScale = 0f;
                AudioManager.Instance.TotalMute = true;
                return;
            }

            switch (value)
            {
                case GameState.Play:
                    Time.timeScale = 1f;
                    Time.fixedDeltaTime = 0.02F * Time.timeScale;
                    AudioManager.Instance.TotalMute = false;
                    break;
                case GameState.Slowed:
                    Time.timeScale = TimeSlow;
                    Time.fixedDeltaTime = 0.02F * Time.timeScale;
                    AudioManager.Instance.TotalMute = false;
                    // If desired, you can slow down the music here.
                    break;
                case GameState.Pause:
                    Time.timeScale = 0f;
                    AudioManager.Instance.TotalMute = true;
                    break;
            }
        }
    }

    // Menu pause sits on top of the regular state: anything that slows or resumes the game while the
    // menu is open only updates the remembered state, which is applied once the menu closes.
    public void SetMenuPaused(bool value)
    {
        _isMenuPaused = value;
        GameState = gameState;
    }

    public void SlowGame(float timeSlow = 0.1f)
    {
        TimeSlow = timeSlow;
        GameState = GameState.Slowed;
    }

    public void PauseGame()
    {
        GameState = GameState.Pause;
    }

    public void ResumeGame()
    {
        GameState = GameState.Play;
    }

    [Header("Hit Stop")]
    [Tooltip("Minimum real-time gap between two hit stops, so rapid contact hits don't freeze the game repeatedly.")]
    [SerializeField] private float _hitStopCooldown = 0.12f;

    private Coroutine _hitStopRoutine;
    private float _hitStopEndTime;
    private float _hitStopReadyTime;

    // Briefly freezes time on impact; the regular game state is re-applied when it ends.
    public void HitStop(float duration, float timeScale = 0.02f, bool ignoreCooldown = false)
    {
        if (duration <= 0f || _isMenuPaused || gameState == GameState.Pause) return;

        if (_hitStopRoutine != null)
        {
            _hitStopEndTime = Mathf.Max(_hitStopEndTime, Time.unscaledTime + duration);
            return;
        }

        if (!ignoreCooldown && Time.unscaledTime < _hitStopReadyTime) return;

        _hitStopEndTime = Time.unscaledTime + duration;
        _hitStopRoutine = StartCoroutine(HitStopRoutine(timeScale));
    }

    private IEnumerator HitStopRoutine(float timeScale)
    {
        Time.timeScale = Mathf.Min(Time.timeScale, timeScale);

        while (Time.unscaledTime < _hitStopEndTime && !_isMenuPaused && gameState != GameState.Pause)
            yield return null;

        _hitStopRoutine = null;
        _hitStopReadyTime = Time.unscaledTime + _hitStopCooldown;
        GameState = gameState;
    }

    private void OnDisable()
    {
        if (_hitStopRoutine == null) return;

        StopCoroutine(_hitStopRoutine);
        _hitStopRoutine = null;
        if (!_isMenuPaused && gameState != GameState.Pause)
            Time.timeScale = gameState == GameState.Slowed ? TimeSlow : 1f;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (GameState == GameState.Pause)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (GameState == GameState.Play)
            {
                SlowGame();
            }
            else if (GameState == GameState.Slowed)
            {
                ResumeGame();
            }
        }
    }
}