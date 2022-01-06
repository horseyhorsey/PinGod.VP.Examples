using Godot;
using static Godot.GD;

public class Game : Node2D
{
	[Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";

	private BarrelMultiball _barrelMultiball;
    private BarrelTargetsMode _barrelTargetsMode;
    private BruceMultiball _bruceMultiball;
    private HurryUpMode _hurryUpMode;
    private bool _lastBall;
    private MultiScoring _multiballScoringMode;
    private OrcaMultiBall _orcaMultiBall;
    private Timer _tiltedTimeOut;
	private JawsBonus endOfBallBonus;
	private PackedScene multiballPkd = null;
	private JawsPinGodGame pinGod;
	private ScoreEntry scoreEntry;

	#region Jaws Variables

	public const int BASIC_SWITCH_VALUE = 10000;
    public const int BONUS_BARREL_VALUE = 500000;
    public const int BONUS_BRUCEY_VALUE = 750000;
    public const int BONUS_SKIPPER_VALUE = 100000;
    public const int BONUS_VALUE = 50000;
    public bool DoublePlayfield { get; set; }
	public bool IsMultiballScoringStarted { get; internal set; }
	public bool KickbackEnabled { get; private set; }
	#endregion

	public override void _EnterTree()
	{
		
		//get packed scene to create an instance of when a Multiball gets activated
		//multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

		pinGod = GetNode("/root/PinGodGame") as JawsPinGodGame;
		pinGod.Connect(nameof(PinGodGame.BallDrained), this, "OnBallDrained");
		pinGod.Connect(nameof(PinGodGame.BallEnded), this, "OnBallEnded");
		pinGod.Connect(nameof(PinGodGame.BallSaved), this, "OnBallSaved");
		pinGod.Connect(nameof(PinGodGame.BonusEnded), this, "OnBonusEnded");
		pinGod.Connect(nameof(PinGodGame.MultiBallEnded), this, "EndMultiball");
		pinGod.Connect(nameof(PinGodGame.ScoreEntryEnded), this, "OnScoreEntryEnded");

		scoreEntry = GetNode("CanvasLayer/ScoreEntry") as ScoreEntry;
		endOfBallBonus = GetNode("CanvasLayer/Bonus") as JawsBonus;

		_multiballScoringMode = GetNode("CanvasLayer/MultiScoring") as MultiScoring;
		_bruceMultiball = GetNode("CanvasLayer/BruceMultiball") as BruceMultiball;
		_barrelMultiball = GetNode("CanvasLayer/BarrelMultiball") as BarrelMultiball;
		_orcaMultiBall = GetNode("CanvasLayer/OrcaMultiball") as OrcaMultiBall;
		_barrelTargetsMode = GetNode("CanvasLayer/BarrelTargetsMode") as BarrelTargetsMode;
		_hurryUpMode = GetNode("CanvasLayer/HurryUpMode") as HurryUpMode;

		_tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		AddChild(_tiltedTimeOut);
		_tiltedTimeOut.Connect("timeout", this, "timeout");

		pinGod.LogInfo("game: enter tree");
	}

	/// <summary>
	/// Handles the trough last switch and other in-lane switches if the game isn't tilted
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return; //game is tilted, don't process other switches when tilted

		if (pinGod.SwitchOn("start", @event))
		{
			pinGod.LogInfo("attract: starting game. started?", pinGod.StartGame());
		}

		//ball shooter, only allow when lane switch on
		if (pinGod.SwitchOn("ball_shooter", @event))
		{
			if (pinGod.SwitchOn("plunger_lane"))
			{
				pinGod.SolenoidPulse("auto_plunger");
				pinGod.SolenoidPulse("flash_wire");
			}
		}
		
		if (pinGod.SwitchOn("sling_l", @event))
		{
			AddPoints(BASIC_SWITCH_VALUE);
			if (!pinGod.IsMultiballRunning) pinGod.SolenoidPulse("flash_sling_l");
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			AddPoints(BASIC_SWITCH_VALUE);
			if (!pinGod.IsMultiballRunning) pinGod.SolenoidPulse("flash_sling_r");
		}
		if (pinGod.SwitchOn("bumper_0", @event))
		{
			AddPoints(BASIC_SWITCH_VALUE);
		}
		if (pinGod.SwitchOn("bumper_1", @event))
		{
			AddPoints(BASIC_SWITCH_VALUE);
		}
		if (pinGod.SwitchOn("bumper_2", @event))
		{
			AddPoints(BASIC_SWITCH_VALUE);
		}
	}

	public override void _Ready()
	{
		pinGod.LogInfo("game: _ready");
		pinGod.DisableAllLamps();
		SetGiState(LightState.On);

		pinGod.BallInPlay = 1;
		StartNewBall();

		pinGod.EnableJawsToy(true);
		ReleaseOrcaMagnet(true);
	}

    /// <summary>
    /// Adds points with double playfield if on
    /// </summary>
    /// <param name="points"></param>
    public int AddPoints(int points, bool update = true)
    {
        var score = DoublePlayfield ? points * 2 : points;
        pinGod.AddPoints(score, update);
        return score;
    }

    public bool IsBruceMultiballRunning()
    {
        if (_bruceMultiball != null)
        {
            return _bruceMultiball.IsMultiballRunning;
        }

        return false;
    }

    /// <summary>
    /// Add a display at end of ball
    /// </summary>
    public void OnBallEnded(bool lastBall)
    {
        pinGod.LogInfo("game: ball ended", pinGod.BallInPlay, "last ball:" + lastBall);
        _lastBall = lastBall;

        if (!pinGod.IsTilted)
        {
            pinGod.InBonusMode = true;
            pinGod.LogInfo("game: adding bonus scene for player: " + pinGod.CurrentPlayerIndex);
            endOfBallBonus.StartBonusDisplay();
            return;
        }
        else if (pinGod.IsTilted && lastBall)
        {
            pinGod.LogInfo("last ball in tilt");
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
                pinGod.LogInfo("Still tilted");
            }
        }
    }

    public void OnBonusEnded()
    {
        pinGod.LogInfo("game: bonus ended, starting new ball");
        pinGod.InBonusMode = false;
        currentPlayer.ResetNewBall();
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

    public void ReleaseOrcaMagnet(bool release = true)
    {
        if (release)
        {
            pinGod.SolenoidOn("orca_magnet", 1);
            pinGod.SolenoidOn("orca_toy", 0);
            pinGod.SolenoidOn("flash_orca_l", 0);
            pinGod.SolenoidOn("flash_orca_m", 0);
            pinGod.SolenoidOn("flash_orca_r", 0);
        }
        else
        {
            pinGod.SolenoidOn("orca_magnet", 0);
            pinGod.SolenoidOn("orca_toy", 1);
            pinGod.SolenoidOn("flash_orca_l", 1);
            pinGod.SolenoidOn("flash_orca_m", 1);
            pinGod.SolenoidOn("flash_orca_r", 1);
        }

        UpdateLamps();
    }

    public void UpdateLamps()
    {
        //update all lamps in groups
        pinGod.UpdateLamps(GetTree());
    }

    /// <summary>
    /// Calls groups with game progress to UpdateProgress
    /// </summary>
    public void UpdateProgress()
    {
        pinGod.LogInfo("updating progress");
        GetTree().CallGroup("game_progress", "UpdateProgress", DoublePlayfield, new int[] { currentPlayer.OrcaMballCompleteCount, currentPlayer.BarrelMballCompleteCount, currentPlayer.BruceMballCompleteCount });
    }

    internal void AddMultiScoringMode()
    {
        _multiballScoringMode.StartScoringMode();
    }

    internal void AwardHurryUp()
    {
        if (IsHurryUpRunning())
        {
            var award = _hurryUpMode.AwardHurryUp();
            award = AddPoints((int)award);
            pinGod.LogInfo("hurryup_awarded: ", award);
        }
    }

    /// <summary>
    /// Enable kickback
    /// </summary>
    /// <param name="enable"></param>
    internal void EnableKickback(bool enable)
    {
        this.KickbackEnabled = enable;
        if (enable) pinGod.SetLampState("kickback", 1);
    }

    internal bool IsHurryUpRunning() => _hurryUpMode?.IsRunning() ?? false;

    /// <summary>
    /// Plays the scene from the Barrel mode to remind player to hit the drop targets
    /// </summary>
    internal void PlayBarrelReminderScene()
    {
        _barrelMultiball?.PlayBarrelReminderScene(false);
    }

    internal void PlayMusicForMode()
    {
        if (currentPlayer.SharkLocksComplete > 0)
        {
            if (currentPlayer.SharkLocksComplete == 2)
                pinGod.PlayMusic("dundun_2");
            else
                pinGod.PlayMusic("dundun_1");
        }
        else
        {
            pinGod.PlayMusic(pinGod.AudioManager.Bgm);
        }
    }

    internal void ResetBarrelDropsRound()
    {
        currentPlayer.BarrelsOn = false;
        currentPlayer.BarrelTargetsUp = false;
        currentPlayer.ResetDropTargets();
        IsJawsLockReady();
    }

    internal void SetGiState(LightState state)
    {
		pinGod.SetLampState("gi_0", (byte)state);
		pinGod.SetLampState("gi_1", (byte)state);
		pinGod.SetLampState("gi_2", (byte)state);
	}

	internal void StartBruceHurryUp()
	{
		_hurryUpMode.StartHurryUp();
	}

	#region Jaws Methods

	public JawsPlayer currentPlayer;

	/// <summary>
	/// Disables the barrel drop target coils
	/// </summary>
	public void DisableBarrelTargets()
	{
		for (int i = 0; i < currentPlayer.DropTargets.Length; i++)
		{
			pinGod.SolenoidOn("drop_" + i, 0);
		}
	}

	public void EnableBruceDiverter(byte enable) => pinGod.SolenoidOn("diverter", enable);
	/// <summary>
	/// Opens the locks if enabled
	/// </summary>
	/// <returns></returns>
	public bool IsJawsLockReady()
	{
		var lockReady = currentPlayer?.SharkLockEnabled ?? false;

		if (currentPlayer != null)
		{
			if (currentPlayer.SharkLockEnabled)
			{
				pinGod.EnableJawsToy(!currentPlayer.SharkLockEnabled);
			}

			if (currentPlayer.QuintLockEnabled) EnableBruceDiverter(1);
			else EnableBruceDiverter(0);
		}

		return lockReady;
	}

    #endregion
    internal void StopHurryUp()
    {
        _hurryUpMode.StopHurryUp();
    }

    void AddMultiballSceneToTree()
    {
		//create an mball instance from the packed scene
		var mball = multiballPkd.Instance();
		//add to multiball group
		mball.AddToGroup("multiball");
		//add to the tree
		GetNode("CanvasLayer").AddChild(mball);
	}

	/// <summary>
	/// Sets <see cref="PinGodGameBase.IsMultiballRunning"/> to false and Any node that is in the multiball group is removed from tree
	/// </summary>
	private void EndMultiball()
	{
		pinGod.LogInfo("removing multiballs");
		GetTree().CallGroup("multiball", "EndMultiball");
		pinGod.PlayMusic("bgmmusic");
		//set coil and send update
		UpdateLamps();
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
	/// <summary>
	/// Called after tilted
	/// </summary>
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
	}

    void timeout()
    {
		if (!_lastBall)
		{
			CallDeferred("OnStartNewBall");
		}
	}
}
