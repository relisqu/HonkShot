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
    private List<ObjectMovementInfo> infos = new List<ObjectMovementInfo>();

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
                    AudioManager.Instance.TotalMute = false;
                    break;
                case GameState.Slowed:
                    Time.timeScale = TimeSlow;
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

    public void SlowGame()
    {
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
        
        if(Input.GetKeyDown(KeyCode.P))
        {
            if(GameState == GameState.Pause)
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
            if(GameState == GameState.Play)
            {
                SlowGame();
            }
            else if (GameState == GameState.Slowed)
            {
                ResumeGame();
            }
            
        }
    }

    #region Depricated
    /// <summary>
    /// Deprivated
    /// </summary>
    public class ObjectMovementInfo
    {
        public Rigidbody2D rb;
        public bool isPlayer;
        public Vector2 UnscaledVelocity;
        public float UnscaledAngularVelocity;
        public Vector2? PrevVelocity;
        public float? PrevAngularVelocity;
    }
    private bool theWorldFlag { get; set; }

    private bool slowtimeFlag = false;

    /// <summary>
    /// Depricated.
    /// </summary>
    private void AddObject(Rigidbody2D rigidbody, bool isPlayer)
    {
        if (rigidbody != null)
        {
            var info = new ObjectMovementInfo();
            info.rb = rigidbody;
            info.isPlayer = isPlayer;
            info.PrevVelocity = null;
            infos.Add(info);
        }
    }


    /// <summary>
    /// Depricated. Yes it is Jojo reference.
    /// </summary>
    private void StartTheWorld()
    {
        foreach (ObjectMovementInfo info in infos)
        {
            info.UnscaledVelocity = info.rb.velocity;
            info.UnscaledAngularVelocity = info.rb.angularVelocity;
        }
        theWorldFlag = true;

    }

    /// <summary>
    /// Depricated. Yes it is Jojo reference.
    /// </summary>
    private void EndTheWorld()
    {
        foreach (ObjectMovementInfo info in infos)
        {
            info.PrevVelocity = null;
            if (!info.isPlayer)
            {
                info.rb.angularVelocity = info.UnscaledAngularVelocity;
                info.rb.velocity = info.UnscaledVelocity;
            }
        }
        theWorldFlag = false;

    }
    // Depricated.
    private void RBSlow()
    {
        // https://www.cyberforum.ru/post13579661.html Source
        foreach (var info in infos)
        {
            if (info.isPlayer) continue;
            if (info.PrevVelocity != null)
            {
                //calc acceleration
                var acc = info.rb.velocity - info.PrevVelocity.Value;

                //calc angular acceleration
                var angularAcc = info.rb.angularVelocity - info.PrevAngularVelocity.Value;

                //assign new velocity
                info.PrevVelocity = info.rb.velocity = info.UnscaledVelocity * TimeSlow;
                info.PrevAngularVelocity = info.rb.angularVelocity = info.UnscaledAngularVelocity * TimeSlow;

                //assign acceleration
                info.UnscaledVelocity += acc;
                info.UnscaledAngularVelocity += angularAcc;
            }
            else
            {
                //first step
                info.PrevVelocity = info.rb.velocity = info.UnscaledVelocity * TimeSlow;
                info.PrevAngularVelocity = info.rb.angularVelocity = info.UnscaledAngularVelocity * TimeSlow;
            }
        }
    }
    #endregion
}
