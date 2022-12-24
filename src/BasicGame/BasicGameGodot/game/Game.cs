using Godot;

/// <summary>
/// Custom Game class for Basic Game
/// </summary>
public class Game : PinGodGameNode
{	
	/// <summary>
	/// Default scene file to use for Multi-Ball
	/// </summary>
	[Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Scenes/Multiball.tscn";

	private bool _lastBall;
	private Timer _tiltedTimeOut;
	private Bonus endOfBallBonus;
	private PackedScene multiballPkd;	
	private ScoreEntry scoreEntry;

	/// <summary>
	/// Connects signals to basic game events, handles tilt time outs
	/// </summary>
	public override void _EnterTree()
	{
		base._EnterTree();

		pinGod.LogInfo("game: enter tree");

		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

		pinGod.Connect(nameof(PinGodGame.BallDrained), this, "OnBallDrained");
		pinGod.Connect(nameof(PinGodGame.BallEnded), this, "OnBallEnded");
		pinGod.Connect(nameof(PinGodGame.BallSaved), this, "OnBallSaved");
		pinGod.Connect(nameof(PinGodGame.BonusEnded), this, "OnBonusEnded");
		pinGod.Connect(nameof(PinGodGame.MultiBallEnded), this, "EndMultiball");
		pinGod.Connect(nameof(PinGodGame.ScoreEntryEnded), this, "OnScoreEntryEnded");
		//pinGod.Connect(nameof(PinGodGameBase.PlayerAdded), this, "OnPlayerAdded");

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

	/// <summary>
	/// Gets an instance of the multi-ball scene and add it to the Modes tree
	/// </summary>
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
	/// <summary>
	/// Displays high score if this is the last ball.
	/// </summary>
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
	/// Sets <see cref="PinGodGame.IsMultiballRunning"/> to false and Any node that is in the multiball group is removed from tree
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
