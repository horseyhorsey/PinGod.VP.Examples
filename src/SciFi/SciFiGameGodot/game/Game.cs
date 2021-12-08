using Godot;
using System;
using static Godot.GD;

public enum GameMusic
{
	None,
	Normal,
	Spawn,
	Invasion,
	Armada,
	AlienBane,
	MultiBall
}

public class Game : Node2D
{
	[Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";
	[Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";

	private bool _lastBall;
	private Timer _tiltedTimeOut;
	private SciFiPlayer currentPlayer;
	private Bonus endOfBallBonus;
	GameMusic GameMusic = GameMusic.None;
	private PackedScene multiballPkd;
	private SciFiPinGodGame pinGod;
	private ScoreEntry scoreEntry;
	public Kickback Kickback { get; private set; }
	public bool KickbackEnabled { get; internal set; }

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as SciFiPinGodGame;
		pinGod.LogInfo("game: enter tree");

		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

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
	}

	public override void _Ready()
	{
		Print("game: _ready");		
		pinGod.DisableAllLamps();
		SetupMode();

		pinGod.LogInfo("game: _ready");
		pinGod.BallInPlay = 1;
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
	
	public void EnableKickback(bool enable = true) => Kickback.EnableKickback(enable);

	/// <summary>
	/// Add a display at end of ball
	/// </summary>
	public void OnBallEnded(bool lastBall)
	{
		pinGod.LogInfo("game: ball ended", pinGod.BallInPlay, "last ball:" + lastBall);
		_lastBall = lastBall;

		EndMultiball();

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

	/// <summary>
	/// Signals to Mode groups OnBallSaved
	/// </summary>
	void OnBallSaved() => pinGod.OnBallSaved(GetTree());

	public void OnBallStarted()
	{
		pinGod.LogInfo("game: ball started");
		pinGod.DisableAllLamps();

		var player = pinGod.GetSciFiPlayer();
		player.ResetNewBall();
		pinGod.ResetTargets();

		SetupMode();

		//EnableKickback(true); //TODO: broken?
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
			pinGod.StartNewBall();
			pinGod.OnBallStarted(GetTree());
			pinGod.UpdateLamps(GetTree());
		}
	}

	public void SetupMode()
	{
		pinGod.LogInfo("game: setting up mode");
		GameMusic = GameMusic.Normal;
		currentPlayer = pinGod.GetSciFiPlayer();
		if (currentPlayer.SpawnEnabled == GameOption.Complete)
		{
			GameMusic = GameMusic.Spawn;
		}
		if (currentPlayer.InvasionEnabled == GameOption.Complete)
		{
			GameMusic = GameMusic.Invasion;
		}
		if (currentPlayer.ArmadaEnabled == GameOption.Complete)
		{
			GameMusic = GameMusic.Armada;
		}
		if (currentPlayer.AlienBaneEnabled == GameOption.Complete)
		{
			GameMusic = GameMusic.AlienBane;
		}

		InitModeLamps(currentPlayer);
		pinGod.AudioManager.StopMusic();
		pinGod.PlayMusic(GameMusic.ToString().ToLower());
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

	private void AddPoints(int points)
	{
		pinGod.AddPoints(points);
		pinGod.AddBonus(25);
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

	private void InitModeLamps(SciFiPlayer sciFiPlayer)
	{
		pinGod.UpdateDockLamps();
		pinGod.UpdatePowerupModeLamps();
		pinGod.UpdateSciFiLamps();
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
	/// When score entry is finished set <see cref="PinGodGame.EndOfGame"/>
	/// </summary>
	void OnScoreEntryEnded()
	{
		pinGod.EndOfGame();
	}

	void OnStartNewBall()
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
