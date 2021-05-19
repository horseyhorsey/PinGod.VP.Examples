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

	public override void _EnterTree()
	{
		Print("game: enter tree");
		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;
	}

	public override void _Ready()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		pinGod.Connect("BallEnded", this, "OnBallEnded");
		pinGod.Connect("BallStarted", this, "OnBallStarted");
		pinGod.Connect("BonusEnded", this, "OnBonusEnded");
		pinGod.Connect("ScoreEntryEnded", this, "OnScoreEntryEnded");

		scoreEntry = GetNode("CanvasLayer/ScoreEntry") as ScoreEntry;
		endOfBallBonus = GetNode("CanvasLayer/Bonus") as Bonus;

		_tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		_tiltedTimeOut.Connect("timeout", this, "timeout");
		AddChild(_tiltedTimeOut);

		Print("game: _ready");
	}

	/// <summary>
	/// Handles the trough last switch and other in-lane switches if the game isn't tilted
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		//check if the input is a trough switch
		var troughSwitch = pinGod.IsTroughSwitch(@event);
		if (troughSwitch > 0)
		{
			//Check if the last trough switch was enabled
			if (troughSwitch == Trough.TroughSwitches[Trough.TroughSwitches.Length - 1])
			{
				Print("trough: switches active");
				//end the ball in play?
				if (!pinGod.InBonusMode && pinGod.GameInPlay && Trough.IsTroughFull() && !Trough.BallSaveActive)
				{
					Print("game: trough switches updated");
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
			}

			if (pinGod.IsMultiballRunning && !Trough.BallSaveActive)
			{
				//this was the last ball in multiball
				if(Trough.BallsInTrough() == 3)
				{
					EndMultiball();
				}
			}
		}

		if (!pinGod.GameInPlay) return;
		//game is tilted, don't process other switches when tilted

		if (pinGod.IsTilted) return;

		if (pinGod.SwitchOn("start", @event))
		{
			Print("attract: starting game. started?", pinGod.StartGame());
		}

		// Flipper switches set to reset the ball search timer
		if (pinGod.SwitchOn("flipper_l", @event))
		{
		}
		if (pinGod.SwitchOn("flipper_r", @event))
		{
		}
		if (pinGod.SwitchOn("outlane_l", @event))
		{
			AddPoints(100);
		}
		if (pinGod.SwitchOn("inlane_l", @event))
		{
			AddPoints(100);
		}
		if (pinGod.SwitchOn("inlane_r", @event))
		{
			AddPoints(100);
		}
		if (pinGod.SwitchOn("outlane_r", @event))
		{
			AddPoints(100);
		}
		if (pinGod.SwitchOn("sling_l", @event))
		{
			AddPoints(50);
		}
		if (pinGod.SwitchOn("sling_r", @event))
		{
			AddPoints(50);
		}
		if (pinGod.SwitchOn("mball_saucer", @event))
		{
			AddPoints(150);
			if (!pinGod.IsMultiballRunning)
			{
				pinGod.IsMultiballRunning = true;
				CallDeferred("AddMultiballSceneToTree");
			}
			pinGod.SolenoidPulse("mball_saucer");
		}
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
		Print("game: ball ended", PinGodGame.BallInPlay, "last ball:" + lastBall);
		_lastBall = lastBall;

		EndMultiball();

		if (!pinGod.IsTilted)
		{
			pinGod.InBonusMode = true;
			Print("game: adding bonus scene for player: " + PinGodGame.CurrentPlayerIndex);
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
		pinGod.StartNewBall();
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
		}
	}

	public void OnBallStarted()
	{
		Print("game: ball started");
	}

	/// <summary>
	/// When score entry is finished set <see cref="PinGodGame.EndOfGame"/>
	/// </summary>
	void OnScoreEntryEnded()
	{
		pinGod.EndOfGame();
	}
}
