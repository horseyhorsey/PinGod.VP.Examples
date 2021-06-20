using Godot;
using static Godot.GD;

public class Game : Node2D
{
	const string MULTIBALL_SCENE = "res://modes/common/multiball/Multiball.tscn";

	private bool _lastBall;
	private Timer _tiltedTimeOut;
	private Bonus endOfBallBonus;
	private PackedScene multiballPkd;
	private PinGodGame pinGod;
	private ScoreEntry scoreEntry;

	public override void _EnterTree()
	{
		Print("game: enter tree");
		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		pinGod.Connect(nameof(PinGodGameBase.BallDrained), this, "OnBallDrained");
		pinGod.Connect(nameof(PinGodGameBase.BallEnded), this, "OnBallEnded");
		pinGod.Connect(nameof(PinGodGameBase.BonusEnded), this, "OnBonusEnded");
		pinGod.Connect(nameof(PinGodGameBase.MultiBallEnded), this, "EndMultiball");
		pinGod.Connect(nameof(PinGodGameBase.ScoreEntryEnded), this, "OnScoreEntryEnded");

		scoreEntry = GetNode("CanvasLayer/ScoreEntry") as ScoreEntry;
		endOfBallBonus = GetNode("CanvasLayer/Bonus") as Bonus;

		_tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		AddChild(_tiltedTimeOut);
		_tiltedTimeOut.Connect("timeout", this, "timeout");
	}

	/// <summary>
	/// Starts a new game and ball (soon as this Game enters the scene tree). OnBallStarted is invoked on nodes grouped as Mode
	/// </summary>
	public override void _Ready()
	{
		Print("game: _ready");

		pinGod.BallInPlay = 1;
		pinGod.StartNewBall();
		//call on ball started on modes that have the method
		pinGod.OnBallStarted(GetTree());
	}

	public void AddMultiballSceneToTree()
	{
		//create an mball instance from the packed scene
		var mball = multiballPkd.Instance();
		//add to multiball group
		mball.AddToGroup("multiball");
		//add to the tree
		GetNode("CanvasLayer").AddChild(mball);
	}
	/// <summary>
	/// Add a display at end of ball
	/// </summary>
	public void OnBallEnded(bool lastBall)
	{
		Print("game: ball ended", pinGod.BallInPlay, "last ball:" + lastBall);
		_lastBall = lastBall;

		EndMultiball();

		if (!pinGod.IsTilted)
		{
			pinGod.InBonusMode = true;
			Print("game: adding bonus scene for player: " + pinGod.CurrentPlayerIndex);
			endOfBallBonus.StartBonusDisplay();
			return;
		}
		else if (pinGod.IsTilted && lastBall)
		{
			Print("last ball in tilt");
			pinGod.InBonusMode = false;
			scoreEntry.DisplayHighScore();
			return;
		}
		else if (pinGod.IsTilted)
		{
			if (_tiltedTimeOut.IsStopped())
			{
				pinGod.InBonusMode = false;
				Print("no bonus, game was tilted. running timer to make player wait");
				_tiltedTimeOut.Start(4);
			}
			else
			{
				Print("Still tilted");
			}
		}
	}
	public void OnBonusEnded()
	{
		Print("game: bonus ended, starting new ball");
		pinGod.InBonusMode = false;
		if (_lastBall)
		{
			Print("game: last ball played, end of game");
			scoreEntry.DisplayHighScore();
		}
		else
		{
			pinGod.StartNewBall();
			//call on ball started on modes that have the method
			pinGod.OnBallStarted(GetTree());
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
		Print("removing multiballs");
		GetTree().CallGroup("multiball", "EndMultiball");
		pinGod.IsMultiballRunning = false;
		//set coil and send update
		//pinGod.SolenoidOn("lampshow_1", 1);
	}
	void OnBallDrained()
	{
		pinGod.IsBallStarted = false;

		if (_tiltedTimeOut.IsStopped())
		{
			if (pinGod.EndBall())
			{
				Print("last ball played game ending");
			}
			else
			{
				Print("game: new ball starting");
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
		Print("game: starting ball after tilting");
		pinGod.StartNewBall();
		//call on ball started on modes that have the method
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
