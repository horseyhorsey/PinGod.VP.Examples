using Godot;
using System;
using static Godot.GD;

public class Game : Node2D
{
	[Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";
	internal const string DRIVINGMODE_SCENE = "res://modes/video/DrivingVideoMode.tscn";

	private bool _lastBall;
	private Timer _mballStartTimer;
	private Timer _tiltedTimeOut;
	private PackedScene drivingMode;
	private Bonus endOfBallBonus;
	private PackedScene multiballPkd;
	private KrPinGodGame pinGod;
	private ScoreEntry scoreEntry;
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as KrPinGodGame;
		pinGod.LogInfo("game: enter tree");

		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;
		drivingMode = ResourceLoader.Load(DRIVINGMODE_SCENE) as PackedScene;

		pinGod.Connect(nameof(PinGodGameBase.BallDrained), this, "OnBallDrained");
		pinGod.Connect(nameof(PinGodGameBase.BallEnded), this, "OnBallEnded");
		pinGod.Connect(nameof(PinGodGameBase.BallSaved), this, "OnBallSaved");
		pinGod.Connect(nameof(PinGodGameBase.BonusEnded), this, "OnBonusEnded");
		pinGod.Connect(nameof(PinGodGameBase.MultiBallEnded), this, "EndMultiball");
		pinGod.Connect(nameof(PinGodGameBase.ScoreEntryEnded), this, "OnScoreEntryEnded");

		scoreEntry = GetNode("Modes/ScoreEntry") as ScoreEntry;
		endOfBallBonus = GetNode("Modes/Bonus") as Bonus;

		_tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		AddChild(_tiltedTimeOut);
		_tiltedTimeOut.Connect("timeout", this, "timeout");

		_mballStartTimer = new Timer() { OneShot = true, WaitTime = 10f, Autostart = false };
		AddChild(_mballStartTimer);
		_mballStartTimer.Connect("timeout", this, "_on_mball_ready_timeout");
		
	}

	/// <summary>
	/// Starts a new game and ball (soon as this Game enters the scene tree). OnBallStarted is invoked on nodes grouped as Mode
	/// </summary>
	public override void _Ready()
	{
		pinGod.LogInfo("game: _ready");
		Input.ParseInputEvent(new InputEventAction() { Action = "truck_up", Pressed = true });
		Input.ParseInputEvent(new InputEventAction() { Action = "truck_down", Pressed = false });
		pinGod.SolenoidOn("truck_diverter", 0); // this wall in VP stops visible locks

		pinGod.BallInPlay = 1;		
		pinGod.PlaySfx("dev_welcome");
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
	public void AddVideoMode()
	{
		var videoMode = drivingMode.Instance() as DrivingVideoMode;
		videoMode.Connect("VideoModeEnded", this, nameof(OnVideoModeEnded));
		pinGod.StopMusic();
		GetNode("Modes").CallDeferred("add_child", videoMode);
	}

	/// <summary>
	/// Add a display at end of ball
	/// </summary>
	public void OnBallEnded(bool lastBall)
	{
		pinGod.LogInfo("game: ball ended", pinGod.BallInPlay, "last ball:" + lastBall);
		_lastBall = lastBall;

		//EndMultiball();

		if (!pinGod.IsTilted)
		{
			pinGod.InBonusMode = true;
			pinGod.LogInfo("game: adding bonus scene for player: " + pinGod.CurrentPlayerIndex);
			endOfBallBonus.StartBonusDisplay();			
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

	internal void StartSuperPursuitMultiball()
	{
		pinGod.StopMusic();
		pinGod.SolenoidOn("show_pursuit", 1);
		pinGod.SolenoidPulse("kit_pursuit"); //car model
		pinGod.LogInfo("starting super pursuit mball..todo");
		pinGod.PlayTvScene("kr_pursuit_mode", "SUPER PURSUIT MULTIBALL\nBALL COUNT\nINCREASES JACKPOTS", 10f);
		_mballStartTimer.Start();
	}

	/// <summary>
	/// Starts multiball on the trough, lets all the balls free from lock
	/// </summary>
	private void _on_mball_ready_timeout()
	{
		pinGod._trough.BallsLocked = 0;
		pinGod.SolenoidOn("show_pursuit", 0);		
		pinGod.SolenoidOn("truck_diverter", 1);
		pinGod.StartMultiBall(4, 25, 1000);
		pinGod.EnableTruckRamp();
		pinGod.PlaySfx("explosion");
		pinGod.PlayMusic("KRtheme");
	}

	private void AddPoints(int points)
	{
		pinGod.AddPoints(points);
		pinGod.AddBonus(5000);
	}

	/// <summary>
	/// Sets <see cref="PinGodGameBase.IsMultiballRunning"/> to false and Any node that is in the multiball group is removed from tree
	/// </summary>
	private void EndMultiball()
	{
		pinGod.LogInfo("removing multiballs");
		GetTree().CallGroup("multiball", "EndMultiball");
		pinGod.IsMultiballRunning = false;

		//reset truck and add completed
		var p = pinGod.Player as KnightRiderPlayer;
		p.ResetTruckActive();
		p.TruckCompleteReady = true; 
		p.TruckCompleteCount++;

		pinGod.SolenoidOn("truck_diverter", 0);
		pinGod.PlayMusic("kr_gameplay");

		pinGod.EnableTruckRamp();		
		p.BillionShotLit = p.AllModesComplete();
		pinGod.UpdateLamps(GetTree());
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

	void OnStartNewBall()
	{
		pinGod.LogInfo("game: starting ball after tilting");
		StartNewBall();
	}

	void OnVideoModeEnded(int score)
	{
		pinGod.LogInfo("video mode ended");
		var vidNode = GetNode("Modes").GetNode("DrivingVideoMode");
		GetNode("Modes").CallDeferred("remove_child", vidNode);
		pinGod.PlayMusic("kr_gameplay");
		pinGod.SolenoidPulse("saucer_truck");
		pinGod.PlaySfx("get_out");
		pinGod.AddPoints(score);
	}
	/// <summary>
	/// Starts new ball in PinGod and invokes OnBallStarted on all Mode groups
	/// </summary>
	/// 
	private void StartNewBall()
	{		
		pinGod.StartNewBall();
		pinGod.PlayMusic("kr_gameplay");		
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
