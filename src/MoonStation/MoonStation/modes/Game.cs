using Godot;
using static Godot.GD;
using System.Threading.Tasks;

public class Game : Node2D
{
	const string MULTIBALL_SCENE = "res://modes/common/multiball/Multiball.tscn";

	private PinGodGame pinGod;
	private ScoreEntry scoreEntry;
	private Bonus endOfBallBonus;
	private PackedScene multiballPkd;
	private bool _lastBall;
	private Timer _tiltedTimeOut;

	#region Game Variables

	public int Multiplier { get; internal set; }
	public byte[] MoonTargets { get; internal set; }
	public byte[] StationTargets { get; internal set; }
	#endregion

	public override void _EnterTree()
	{
		Print("game: enter tree");
		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		pinGod.Connect(nameof(PinGodGameBase.BallDrained), this, "OnBallDrained");
		pinGod.Connect(nameof(PinGodGameBase.BallEnded), this, "OnBallEnded");
		pinGod.Connect(nameof(PinGodGameBase.BonusEnded), this, "OnBonusEnded");
		pinGod.Connect(nameof(PinGodGameBase.ScoreEntryEnded), this, "OnScoreEntryEnded");

		_tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		AddChild(_tiltedTimeOut);
		_tiltedTimeOut.Connect("timeout", this, "timeout");

		scoreEntry = GetNode("CanvasLayer/ScoreEntry") as ScoreEntry;
		endOfBallBonus = GetNode("CanvasLayer/Bonus") as Bonus;
	}

	public override void _Ready()
	{
		Print("game: _ready");
		OnGameStarted();
	}

	private void AddPoints(int points)
	{
		pinGod.AddPoints(points);
		pinGod.AddBonus(25);
	}

	void AddMultiballSceneToTree()
	{
		//create an mball instance from the packed scene
		var mball = multiballPkd.Instance();
		//add to multiball group
		mball.AddToGroup("multiball");
		//add to the tree
		GetNode("CanvasLayer").AddChild(mball);

		CallDeferred("PlayShow", 1);
	}

	//play a lampshow (VP LightSeq)
	void PlayShow(int num) => pinGod.SolenoidOn("lampshow_"+num, 1);


	/// <summary>
	/// Add a display at end of ball
	/// </summary>
	public void OnBallEnded(bool lastBall)
	{
		pinGod.AudioManager.StopMusic();

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

	void timeout()
	{		
		if (!_lastBall)
		{
			CallDeferred("OnStartNewBall");
		}
	}

	/// <summary>
	/// Sets <see cref="PinGodGameBase.IsMultiballRunning"/> to false and Any node that is in the multiball group is removed from tree
	/// </summary>
	private void EndMultiball()
	{
		Print("removing multiballs");
		GetTree().CallGroup("multiball", "EndMultiball");
		pinGod.IsMultiballRunning = false;
		pinGod.SolenoidOn("lampshow_1", 1);
	}

	void OnStartNewBall()
	{
		Print("game: starting ball after tilting");
		pinGod.SolenoidOn("lampshow_1", 0);
		pinGod.StartNewBall();
		//call on ball started on modes that have the method
		pinGod.OnBallStarted(GetTree());
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

	void OnBallDrained()
	{
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

	void OnGameStarted()
	{
		Print("game: game started");
		if (pinGod.AudioManager.MusicEnabled)
		{
			Print("playing music");
			pinGod.AudioManager.PlayMusic(pinGod.AudioManager.Bgm);
		}
	}

	/// <summary>
	/// When score entry is finished set <see cref="PinGodGame.EndOfGame"/>
	/// </summary>
	void OnScoreEntryEnded()
	{
		pinGod.EndOfGame();
	}

	public void UpdateLamps()
	{
		if (Multiplier > 1)
		{
			pinGod.SetLampState("multiplier_2", 2);

			if (Multiplier > 2)
			{
				pinGod.SetLampState("multiplier_2", 1);
				pinGod.SetLampState("multiplier_3", 2);
			}
			if (Multiplier > 3)
			{
				pinGod.SetLampState("multiplier_2", 1);
				pinGod.SetLampState("multiplier_3", 1);
				pinGod.SetLampState("multiplier_4", 2);
			}
		}
		else
		{
			pinGod.SetLampState("multiplier_2", 0);
			pinGod.SetLampState("multiplier_3", 0);
			pinGod.SetLampState("multiplier_4", 0);
		}
	}
}
