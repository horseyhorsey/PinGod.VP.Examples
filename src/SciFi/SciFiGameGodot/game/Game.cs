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

public class Game : PinGodGameNode
{
	[Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";

	private bool _lastBall;
	private Timer _tiltedTimeOut;
	private SciFiPlayer currentPlayer;
	private Bonus endOfBallBonus;
	GameMusic GameMusic = GameMusic.None;
	private PackedScene multiballPkd;
	private SciFiPinGodGame sciPinGod;
	private ScoreEntry scoreEntry;
	public Kickback Kickback { get; private set; }
	public bool KickbackEnabled { get; internal set; }

	public override void _EnterTree()
	{
		sciPinGod = GetNode("/root/PinGodGame") as SciFiPinGodGame;
		sciPinGod.LogInfo("game: enter tree");

		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

		sciPinGod.Connect(nameof(PinGodGame.BallDrained), this, "OnBallDrained");
		sciPinGod.Connect(nameof(PinGodGame.BallEnded), this, "OnBallEnded");
		sciPinGod.Connect(nameof(PinGodGame.BallSaved), this, "OnBallSaved");
		sciPinGod.Connect(nameof(PinGodGame.BonusEnded), this, "OnBonusEnded");
		sciPinGod.Connect(nameof(PinGodGame.MultiBallEnded), this, "EndMultiball");
		sciPinGod.Connect(nameof(PinGodGame.ScoreEntryEnded), this, "OnScoreEntryEnded");

		scoreEntry = GetNode("Modes/ScoreEntry") as ScoreEntry;
		endOfBallBonus = GetNode("Modes/Bonus") as Bonus;

		_tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		AddChild(_tiltedTimeOut);
		_tiltedTimeOut.Connect("timeout", this, "timeout");		
	}

	public override void _Ready()
	{
		Print("game: _ready");		
		sciPinGod.DisableAllLamps();
		SetupMode();

		sciPinGod.LogInfo("game: _ready");
		sciPinGod.BallInPlay = 1;
		StartNewBall();		
	}

	/// <summary>
	/// Starts new ball in PinGod and invokes OnBallStarted on all Mode groups
	/// </summary>
	private void StartNewBall()
	{
		sciPinGod.DisableAllLamps();
		sciPinGod.StartNewBall();
		sciPinGod.OnBallStarted(GetTree());
	}
	
	public void EnableKickback(bool enable = true) => Kickback.EnableKickback(enable);

	/// <summary>
	/// Add a display at end of ball
	/// </summary>
	public void OnBallEnded(bool lastBall)
	{
		sciPinGod.LogInfo("game: ball ended", sciPinGod.BallInPlay, "last ball:" + lastBall);
		_lastBall = lastBall;

		EndMultiball();

		if (!sciPinGod.IsTilted)
		{
			sciPinGod.InBonusMode = true;
			sciPinGod.LogInfo("game: adding bonus scene for player: " + sciPinGod.CurrentPlayerIndex);
			endOfBallBonus.StartBonusDisplay();
			return;
		}
		else if (sciPinGod.IsTilted && lastBall)
		{
			sciPinGod.LogDebug("last ball in tilt");
			sciPinGod.InBonusMode = false;
			scoreEntry.DisplayHighScore();
			return;
		}
		else if (sciPinGod.IsTilted)
		{
			if (_tiltedTimeOut.IsStopped())
			{
				sciPinGod.InBonusMode = false;
				sciPinGod.LogInfo("no bonus, game was tilted. running timer to make player wait");
				_tiltedTimeOut.Start(4);
			}
			else
			{
				sciPinGod.LogDebug("Still tilted");
			}
		}
	}

	/// <summary>
	/// Signals to Mode groups OnBallSaved
	/// </summary>
	void OnBallSaved() => sciPinGod.OnBallSaved(GetTree());

	public void OnBallStarted()
	{
		sciPinGod.LogInfo("game: ball started");
		sciPinGod.DisableAllLamps();

		var player = sciPinGod.GetSciFiPlayer();
		player.ResetNewBall();
		sciPinGod.ResetTargets();

		SetupMode();

		//EnableKickback(true); //TODO: broken?
	}

	public void OnBonusEnded()
	{
		sciPinGod.LogInfo("game: bonus ended, starting new ball");
		sciPinGod.InBonusMode = false;
		if (_lastBall)
		{
			sciPinGod.LogInfo("game: last ball played, end of game");
			scoreEntry.DisplayHighScore();
		}
		else
		{
			sciPinGod.StartNewBall();
			sciPinGod.OnBallStarted(GetTree());
			sciPinGod.UpdateLamps(GetTree());
		}
	}

	public void SetupMode()
	{
		sciPinGod.LogInfo("game: setting up mode");
		GameMusic = GameMusic.Normal;
		currentPlayer = sciPinGod.GetSciFiPlayer();
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
		sciPinGod.AudioManager.StopMusic();
		sciPinGod.PlayMusic(GameMusic.ToString().ToLower());
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
		sciPinGod.AddPoints(points);
		sciPinGod.AddBonus(25);
	}

	/// <summary>
	/// Sets <see cref="PinGodGameBase.IsMultiballRunning"/> to false and Any node that is in the multiball group is removed from tree
	/// </summary>
	private void EndMultiball()
	{
		sciPinGod.LogInfo("removing multiballs");
		GetTree().CallGroup("multiball", "EndMultiball");
		sciPinGod.IsMultiballRunning = false;
	}

	private void InitModeLamps(SciFiPlayer sciFiPlayer)
	{
		sciPinGod.UpdateDockLamps();
		sciPinGod.UpdatePowerupModeLamps();
		sciPinGod.UpdateSciFiLamps();
	}

	void OnBallDrained()
	{
		if (_tiltedTimeOut.IsStopped())
		{
			sciPinGod.OnBallDrained(GetTree());

			if (sciPinGod.EndBall())
			{
				sciPinGod.LogInfo("last ball played game ending");
			}
			else
			{
				sciPinGod.LogInfo("game: new ball starting");
			}
		}
	}

	/// <summary>
	/// When score entry is finished set <see cref="PinGodGame.EndOfGame"/>
	/// </summary>
	void OnScoreEntryEnded()
	{
		sciPinGod.EndOfGame();
	}

	void OnStartNewBall()
	{
		sciPinGod.DisableAllLamps();
		sciPinGod.StartNewBall();
		sciPinGod.OnBallStarted(GetTree());
	}
	
	void timeout()
	{		
		if (!_lastBall)
		{
			CallDeferred("OnStartNewBall");
		}
	}
}
