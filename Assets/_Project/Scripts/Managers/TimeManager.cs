using Scripts.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectMovementInfo
{
    public Rigidbody2D rb;
    public bool isPlayer;
    public Vector2 UnscaledVelocity;
    public float UnscaledAngularVelocity;
    public Vector2? PrevVelocity;
    public float? PrevAngularVelocity;
}
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    private float timeSlow = 0.1f;
    private List<ObjectMovementInfo> infos = new List<ObjectMovementInfo>();
    private bool theWorldFlag {  get; set; }

    public void AddObject(Rigidbody2D rigidbody, bool isPlayer)
    {
        if(rigidbody != null)
        {
            var info = new ObjectMovementInfo();
            info.rb = rigidbody;
            info.isPlayer = isPlayer;
            info.PrevVelocity = null;
            infos.Add(info);
        }
    }


    /// <summary>
    /// Yes it is Jojo reference.
    /// </summary>
    public void StartTheWorld()
    {
        foreach (ObjectMovementInfo info in infos)
        {
            info.UnscaledVelocity = info.rb.velocity;
            info.UnscaledAngularVelocity = info.rb.angularVelocity;
        }
        theWorldFlag = true;
        
    }

    public void EndTheWorld()
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

    public bool IsPaused {  get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void FixedUpdate()
    {
        if (theWorldFlag)
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
                    info.PrevVelocity = info.rb.velocity = info.UnscaledVelocity * timeSlow;
                    info.PrevAngularVelocity = info.rb.angularVelocity = info.UnscaledAngularVelocity * timeSlow;

                    //assign acceleration
                    info.UnscaledVelocity += acc;
                    info.UnscaledAngularVelocity += angularAcc;
                }
                else
                {
                    //first step
                    info.PrevVelocity = info.rb.velocity = info.UnscaledVelocity * timeSlow;
                    info.PrevAngularVelocity = info.rb.angularVelocity = info.UnscaledAngularVelocity * timeSlow;
                }
            }
        }
    }
    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        IsPaused = true;
        AudioManager.Instance.SetMute(SoundChanelType.Player, true);
        AudioManager.Instance.SetMute(SoundChanelType.Environment, true);
        AudioManager.Instance.SetMute(SoundChanelType.Enemy, true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        IsPaused = false;
        AudioManager.Instance.SetMute(SoundChanelType.Player, false);
        AudioManager.Instance.SetMute(SoundChanelType.Environment, false);
        AudioManager.Instance.SetMute(SoundChanelType.Enemy, false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (theWorldFlag)
            {
                EndTheWorld();
            }
            else
            {
                StartTheWorld();
            }
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            if(IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
}
