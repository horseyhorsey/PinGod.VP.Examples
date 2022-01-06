using Godot;
using System;
using System.Linq;
using static Godot.GD;

public class Game : Node2D
{
	[Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";

	private bool _lastBall;
	private Timer _tiltedTimeOut;
	private Bonus endOfBallBonus;
	private PackedScene multiballPkd;
	private PinGodGame pinGod;
	private ScoreEntry scoreEntry;
	public GremlinsPlayer _player;

	public bool ModeRunning { get; internal set; }
	public bool WizardIsStarted { get; internal set; }

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		pinGod.LogInfo("game: enter tree");

		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

		pinGod.Connect(nameof(PinGodGame.BallDrained), this, "OnBallDrained");
		pinGod.Connect(nameof(PinGodGame.BallEnded), this, "OnBallEnded");
		pinGod.Connect(nameof(PinGodGame.BallSaved), this, "OnBallSaved");
		pinGod.Connect(nameof(PinGodGame.BonusEnded), this, "OnBonusEnded");
		pinGod.Connect(nameof(PinGodGame.MultiBallEnded), this, "EndMultiball");
		pinGod.Connect(nameof(PinGodGame.ScoreEntryEnded), this, "OnScoreEntryEnded");

		scoreEntry = GetNode("Modes/ScoreEntry") as ScoreEntry;
		endOfBallBonus = GetNode("Modes/Bonus") as Bonus;

		_tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		AddChild(_tiltedTimeOut);
		_tiltedTimeOut.Connect("timeout", this, "timeout");
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

	internal void AddToSpiderJackpot()
	{
		//todo: add jackpot
	}

	internal GremlinMode RotateModes()
	{
		if (_player.CanRotateModes)
		{
			if (!_player.AllModesCompleted)
			{
				//get uncompleted modes and select random
				var modes = _player.CompletedModes.Where(x => !x.Completed);
				Randomize();
				int rand = (int)RandRange(0, modes.Count());

				pinGod.LogDebug($"rotating mode, rand: {rand}, modeCnt: {modes.Count()}");
				_player.SelectedMode = modes.ElementAt(rand);
				return _player.SelectedMode;
			}
		}        

		return null;
	}

	internal void AddSelectedGremlinsWizardMode()
	{
		
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
	void timeout()
	{		
		if (!_lastBall)
		{
			CallDeferred("OnStartNewBall");
		}
	}
	/// <summary>
	/// Starts new ball in PinGod and invokes OnBallStarted on all Mode groups
	/// </summary>
	private void StartNewBall()
	{
		pinGod.DisableAllLamps();
		pinGod.StartNewBall();
		_player = pinGod.Player as GremlinsPlayer;
		pinGod.OnBallStarted(GetTree());		
	}

	public void AddSelectedGremlinsMode()
	{
		ModeRunning = true;
	}
}
