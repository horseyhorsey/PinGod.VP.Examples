using Godot;
using System;

public class Game : PinGodGameNode
{
    [Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";

    private bool _lastBall;
    private Timer _tiltedTimeOut;
    private Node2D displayMsgContainer;
    private Bonus endOfBallBonus;
    bool isLastBall = false;
    private PackedScene multiballPkd;
    private ScoreEntry scoreEntry;
    private Control scoreNode;

    #region Nightmare property add on
    public AudioStreamPlayer bgmMusicPlayer;
    private PackedScene _midnightModePacked;
    private Timer displayTimer;
    private Label nightmareMsg;
    private NightmarePlayer _player;

    public float MusicPauseTime { get; set; }
    public bool RemixSoundsMode { get; internal set; }
    public Timer resumeBgmTimer { get; private set; }
    #endregion

    public override void _EnterTree()
    {
        base._EnterTree();
        pinGod.LogInfo("game: enter tree");

        //get packed scene to create an instance of when a Multiball gets activated
        multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

        _midnightModePacked = ResourceLoader.Load("res://modes/custom/Midnight.tscn") as PackedScene;

        pinGod.Connect(nameof(PinGodGameBase.BallDrained), this, "OnBallDrained");
        pinGod.Connect(nameof(PinGodGameBase.BallEnded), this, "OnBallEnded");
        pinGod.Connect(nameof(PinGodGameBase.BonusEnded), this, "OnBonusEnded");
        pinGod.Connect(nameof(PinGodGameBase.MultiBallEnded), this, "EndMultiball");
        pinGod.Connect(nameof(PinGodGameBase.ScoreEntryEnded), this, "OnScoreEntryEnded");

        scoreEntry = GetNode("Modes/ScoreEntry") as ScoreEntry;
        endOfBallBonus = GetNode("Modes/Bonus") as Bonus;

        _tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
        AddChild(_tiltedTimeOut);
        _tiltedTimeOut.Connect("timeout", this, "timeout");

        resumeBgmTimer = GetNode<Timer>("ResumeMusicTimer");

        displayTimer = GetNode<Timer>("DisplayMessageTimer");
        displayTimer.Connect("timeout", this, "OnDisplayTimeout");
        displayMsgContainer = GetNode("Modes/Node2D") as Node2D;
        nightmareMsg = GetNode<Label>("Modes/Node2D/NightmareMessage");
        displayMsgContainer.Visible = false;

        bgmMusicPlayer = GetNode<AudioStreamPlayer>("BgmMusicPlayer");
        PauseBgm();
    }

    /// <summary>
    /// Update lamps in the mode
    /// </summary>
    internal void UpdateLamps()
    {
        pinGod.UpdateLamps(GetTree());
    }

    internal void StartHurryUp(bool music)
    {                        
        GetNode<HurryUp>("Modes/HurryUp").StartHurryUp();

        if (music)
        {
            pinGod.PlayMusic("mus_hurryup");
        }            
    }

    public void _on_StartBallTimer_timeout()
    {
        pinGod.StartNewBall();
        pinGod.OnBallStarted(GetTree());
        pinGod.UpdateLamps(GetTree());
        _player = (pinGod as NightmarePinGodGame).GetPlayer();
    }

    public override void _Ready()
    {
        pinGod.DisableAllLamps();
        pinGod.LogInfo("game: _ready");
        pinGod.BallInPlay = 1;
        StartOfBallMusic();
    }

    public void EndOfBallTimer_timeout()
    {

        pinGod.LogInfo("end of ball timed out");
        BallEnded(isLastBall);
    }

    /// <summary>
    /// Add a display at end of ball
    /// </summary>
    public void OnBallEnded(bool lastBall)
    {
        isLastBall = lastBall;

    }

    public void OnBonusEnded()
    {
        pinGod.LogInfo("game: bonus ended, starting new ball");
        pinGod.InBonusMode = false;
        if (_lastBall)
        {
            pinGod.LogInfo("game: last ball played, end of game");
            scoreEntry.DisplayHighScore();
        }
        else
        {
            StartOfBallMusic();
        }
    }

    public void OnDisplayMessage(string message, float time = 1.5f)
    {
        displayTimer.Stop();
        displayTimer.Start(time);
        displayMsgContainer.Visible = true;
        nightmareMsg.Text = message;

        pinGod.LogDebug("display message: " + message);
    }

    /// <summary>
    /// disable bollocks
    /// </summary>
    internal void DisableBallLocks(bool enableSuper)
    {
        pinGod.LogInfo("disabling ball locks");
        var p = (NightmarePlayer)pinGod.Player;
        p.DisableBallLocks(enableSuper);        
    }

    /// <summary>
    /// Pauses the <see cref="bgmMusicPlayer"/>
    /// </summary>
    /// <param name="pause"></param>
    internal void PauseBgm(bool pause = true) => bgmMusicPlayer.StreamPaused = pause;
    /// <summary>
    /// Stops the music, saves the time ...plays new music sound and resumes after
    /// </summary>
    /// <param name="music"></param>
    /// <param name="resumeDelay"></param>	
    internal void PlayThenResumeMusic(string music, float resumeDelay)
    {
        if (((NightmarePlayer)pinGod.Player).MidnightRunning) { resumeBgmTimer.Stop(); return; }

        resumeBgmTimer.Stop();
        PauseBgm();        

        pinGod.PlayMusic(music);
        resumeBgmTimer.Start(resumeDelay);
    }

    internal void StartMidnight()
    {
        GetNode("Modes").AddChild(_midnightModePacked.Instance());
    }

    private void _on_ResumeMusicTimer_timeout()
    {
        if (!_player.MidnightRunning)
        {
            pinGod.StopMusic();
            PauseBgm(false);
        }         
    }

    void AddMultiballSceneToTree()
    {
        //create an mball instance from the packed scene
        var mball = multiballPkd.Instance();
        //add to multiball group
        mball.AddToGroup("multiball");
        //add to the tree
        GetNode("Modes").AddChild(mball);
    }

    private void AddPoints(int points)
    {
        pinGod.AddPoints(points);
    }

    private void BallEnded(bool lastBall)
    {
        pinGod.LogInfo("game: ball ended", pinGod.BallInPlay, "last ball:" + lastBall);
        PauseBgm(true);
        _lastBall = lastBall;
        EndMultiball();

        if (!pinGod.IsTilted)
        {
            pinGod.InBonusMode = true;
            pinGod.LogInfo("game: adding bonus scene for player: " + pinGod.CurrentPlayerIndex);
            StartBonusDisplay();
            return;
        }
        else if (pinGod.IsTilted && lastBall)
        {
            pinGod.LogDebug("last ball in tilt");
            pinGod.InBonusMode = false;
            scoreEntry.DisplayHighScore();
            return;
        }
        else if (pinGod.IsTilted)
        {
            if (_tiltedTimeOut.IsStopped())
            {
                pinGod.InBonusMode = false;
                pinGod.LogInfo("no bonus, game was tilted. running timer to make player wait");
                _tiltedTimeOut.Start(4);
            }
            else
            {
                pinGod.LogDebug("Still tilted");
            }
        }
    }

    public void StartBonusDisplay()
    {
        endOfBallBonus.CallDeferred("StartBonusDisplay");
    }

    /// <summary>
    /// Sets <see cref="PinGodGameBase.IsMultiballRunning"/> to false and Any node that is in the multiball group is removed from tree
    /// </summary>
    private void EndMultiball()
    {
        pinGod.LogInfo("removing multiballs");
        GetTree().CallGroup("multiball", "EndMultiball");
        pinGod.IsMultiballRunning = false;
    }

    void OnBallDrained()
    {
        if (_tiltedTimeOut.IsStopped())
        {
            pinGod.OnBallDrained(GetTree());

            if (pinGod.EndBall())
            {
                pinGod.LogInfo("last ball played game ending");
            }
            else
            {
                pinGod.LogInfo("game: new ball starting");
            }

            OnDisplayMessage("SCARED\nSTIFF", 2f);
            GetNode<Timer>("EndOfBallTimer").Start();
        }
    }

    void OnDisplayTimeout()
    {
        displayMsgContainer.Visible = false;
        pinGod.SolenoidOn("vpcoil", 255); //send value to reset other "on" coils
    }

    /// <summary>
    /// When score entry is finished set <see cref="PinGodGame.EndOfGame"/>
    /// </summary>
    void OnScoreEntryEnded()
    {
        pinGod.EndOfGame();
    }

    void OnStartNewBall()
    {
        pinGod.LogInfo("game: starting ball after tilting");
        pinGod.StartNewBall();
        pinGod.OnBallStarted(GetTree());        
    }

    private void StartOfBallMusic()
    {
        pinGod.PlayMusic("mus_ballready");
        GetNode<Timer>("StartBallTimer").Start();
    }
    void timeout()
    {
        if (!_lastBall)
        {
            CallDeferred("OnStartNewBall");
        }
    }
}
