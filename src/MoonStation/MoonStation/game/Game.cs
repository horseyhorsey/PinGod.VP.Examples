using Godot;

public class Game : Node2D
{
	[Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";

	private bool _lastBall;
	private Timer _tiltedTimeOut;
	private Bonus endOfBallBonus;
	private PackedScene multiballPkd;
	private MsPinGodGame pinGod;
	private ScoreEntry scoreEntry;

	#region Moon Station Game Specific
	public byte[] MoonTargets { get; internal set; }	
	public byte[] StationTargets { get; internal set; }
	#endregion

	public override void _EnterTree()
	{
		pinGod = GetNode<MsPinGodGame>("/root/PinGodGame");
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

			//using CallDeferred here. When in Visual pinball the screen would disappear after adding a moon image.
			//so it could be the more heavier the scene is you will need to CallDeferred. You won't notice this until playing with simulator.
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

	public void UpdateLamps()
	{
		if (pinGod.Multiplier > 1)
		{
			pinGod.SetLampState("multiplier_2", 2);

			if (pinGod.Multiplier > 2)
			{
				pinGod.SetLampState("multiplier_2", 1);
				pinGod.SetLampState("multiplier_3", 2);
			}
			if (pinGod.Multiplier > 3)
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

	/// <summary>
	/// Sets <see cref="PinGodGameBase.IsMultiballRunning"/> to false and Any node that is in the multiball group is removed from tree
	/// </summary>
	private void EndMultiball()
	{
		pinGod.LogInfo("removing multiballs");
		GetTree().CallGroup("multiball", "EndMultiball");
		pinGod.IsMultiballRunning = false;
		pinGod.SolenoidOn("lampshow_1", 1);
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
	/// Sends OnBallSaved to all scenes in the Mode group
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
		pinGod.SolenoidOn("lampshow_1", 0);
		StartNewBall();
	}

    /// <summary>
    /// Starts new ball in PinGod and invokes OnBallStarted on all Mode groups
    /// </summary>
    private void StartNewBall()
    {
        //disable lamps, we're not using led in this game
        pinGod.DisableAllLamps();

        //start the ball
        pinGod.StartNewBall();

        //let all scenes that are in the Mode group that the ball has started
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
