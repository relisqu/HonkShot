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

    public GameState GameState
    {
        get { return gameState; }
        private set
        {
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
                    // If desired, you can slow down the music here.
                    break;
                case GameState.Pause:
                    Time.timeScale = 0f;
                    AudioManager.Instance.TotalMute = true;
                    break;
            }

            gameState = value;
        }
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