using Godot;
using System;

public class Game : PinGodGameNode
{
    [Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";
    [Export] Godot.Collections.Array<AudioStream> _skeleStreams;

    private bool _lastBall;
    private Timer _tiltedTimeOut;
    private Bonus endOfBallBonus;
    private PackedScene multiballPkd;
    private ScoreEntry scoreEntry;
    private MotuPlayer _player;
    private Timer _skeleVoiceTimer;
    private AudioStreamPlayer _skeleAudio;
    private BaseMode _baseMode;

    public override void _EnterTree()
    {
        base._EnterTree();

        pinGod.LogInfo("game: enter tree");

        //get packed scene to create an instance of when a Multiball gets activated
        multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

        pinGod.Connect(nameof(PinGodGameBase.BallDrained), this, "OnBallDrained");
        pinGod.Connect(nameof(PinGodGameBase.BallEnded), this, "OnBallEnded");
        pinGod.Connect(nameof(PinGodGameBase.BallSaved), this, "OnBallSaved");
        pinGod.Connect(nameof(PinGodGameBase.BonusEnded), this, "OnBonusEnded");
        pinGod.Connect(nameof(PinGodGameBase.MultiBallEnded), this, "EndMultiball");
        pinGod.Connect(nameof(PinGodGameBase.ScoreEntryEnded), this, "OnScoreEntryEnded");
        //pinGod.Connect(nameof(PinGodGameBase.PlayerAdded), this, "OnPlayerAdded");

        scoreEntry = GetNode("Modes/ScoreEntry") as ScoreEntry;
        endOfBallBonus = GetNode("Modes/Bonus") as Bonus;

        _tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
        AddChild(_tiltedTimeOut);
        _tiltedTimeOut.Connect("timeout", this, "timeout");

        //skele voice timer
        _skeleVoiceTimer = new Timer() { OneShot = false, WaitTime = 12, Autostart = true };
        AddChild(_skeleVoiceTimer);
        _skeleVoiceTimer.Connect("timeout", this, "skele_timeout");

        _skeleAudio = GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));

        _baseMode = GetNode<BaseMode>("Modes/BaseMode");
    }

    RandomNumberGenerator rand = new RandomNumberGenerator();

    /// <summary>
    /// Plays random skeletor call outs
    /// </summary>
    void skele_timeout()
    {
        if (!pinGod?.IsBallStarted ?? false) return;
        if (_player.IsSorceressRunning) return;
        if (_player.IsSkeleMultiballRunning || _player.IsGrayskullMultiballRunning || _player.IsRipperMultiballRunning) return;
        if (pinGod.SwitchOn("heman_scoop")) return;

        _skeleAudio.Stream = _skeleStreams[rand.RandiRange(0, _skeleStreams.Count - 1)];
        _skeleAudio.Play();
    }

    /// <summary>
    /// Starts a new game and ball (soon as this Game enters the scene tree). OnBallStarted is invoked on nodes grouped as Mode
    /// </summary>
    public override void _Ready()
    {
        pinGod.LogInfo("game: _ready");
        pinGod.BallInPlay = 1;
        StartNewBall();
    }

    public void AddMultiballSceneToTree()
    {
        //create an mball instance from the packed scene
        var mball = multiballPkd.Instance();
        //add to multiball group
        mball.AddToGroup("multiball");
        //add to the tree
        GetNode("Modes").AddChild(mball);
    }

    /// <summary>
    /// Add a display at end of ball
    /// </summary>
    public void OnBallEnded(bool lastBall)
    {
        pinGod.LogInfo("game: ball ended ", pinGod.BallInPlay, " last ball:" + lastBall);
        _lastBall = lastBall;

        //EndMultiball(); todo: remove this from all games?

        if (!pinGod.IsTilted)
        {
            pinGod.InBonusMode = true;
            //pinGod.LogInfo("game: adding bonus scene for player: " + pinGod.CurrentPlayerIndex);
            endOfBallBonus.StartBonusDisplay(false);
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
            StartNewBall();
            pinGod.UpdateLamps(GetTree());
        }
    }

    public void PlayBackgroundMusic()
    {
        if (_player.IsRotonReady)
        {
            pinGod.PlayMusic("creeping");

            if (!_player.GrayskullModeEnding || !_player.IsRipperModeEnding)
                (pinGod as MotuPinGodGame).SkellyTargetBankController();
        }
        else
        {
            pinGod.PlayMusic("theme_alt");
        }
    }

    /// <summary>
    /// Displays sorc ready
    /// </summary>
    /// <returns></returns>
    internal bool StartSorceress()
    {
        if (_player.IsSorceressReady())
        {
            pinGod.PlaySfx("sorc_waste_no_time");
            _baseMode.PlayThenClearAnimation("plunger_lane", "SORCERESS", "SHOOT HE-MAN SCOOP TO START");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets <see cref="PinGodGameBase.IsMultiballRunning"/> to false and Any node that is in the multiball group is removed from tree
    /// </summary>
    private void EndMultiball()
    {
        PlayBackgroundMusic();
        pinGod.LogInfo("game: removing multiballs");
        GetTree().CallGroup("multiball", "EndMultiball");
    }

    void OnBallDrained()
    {
        pinGod.StopMusic();
        _skeleVoiceTimer.Stop();
        _skeleAudio.Stop();

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
        }
    }
    /// <summary>
    /// Signals to Mode groups OnBallSaved
    /// </summary>
    void OnBallSaved() => pinGod.OnBallSaved(GetTree());
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
        StartNewBall();
    }
    /// <summary>
    /// Starts new ball in PinGod and invokes OnBallStarted on all Mode groups
    /// </summary>
    private void StartNewBall()
    {
        pinGod.DisableAllLamps();
        pinGod.StartNewBall();
        pinGod.OnBallStarted(GetTree());
        //pinGod.PlayMusic("theme_alt");
        _player = (pinGod as MotuPinGodGame).CurrentPlayer();
        _skeleVoiceTimer.Start();
    }

    void timeout()
    {
        if (!_lastBall)
        {
            CallDeferred("OnStartNewBall");
        }
    }

    internal void StopSkeleSounds()
    {
        _skeleVoiceTimer.Stop();
        _skeleAudio.Stop();
    }
}
