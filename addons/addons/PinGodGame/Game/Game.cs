using Godot;

/// <summary>
/// Custom Game class for Basic Game
/// </summary>
public abstract class Game : PinGodGameNode
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

		pinGod.LogInfo("game: _enterTree. Getting ScoreEntry and Bonus scenes from the mode tree");
        scoreEntry = GetNode("Modes/ScoreEntry") as ScoreEntry;
        endOfBallBonus = GetNode("Modes/Bonus") as Bonus;

        pinGod.LogInfo("game: loading multiball scene:  " + MULTIBALL_SCENE);
        multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

        pinGod.LogInfo("game: connecting game signals");
        pinGod.Connect(nameof(PinGodGame.BallDrained), this, nameof(OnBallDrained));
		pinGod.Connect(nameof(PinGodGame.BallEnded), this, nameof(OnBallEnded));
		pinGod.Connect(nameof(PinGodGame.BallSaved), this, nameof(OnBallSaved));
		pinGod.Connect(nameof(PinGodGame.BonusEnded), this, nameof(OnBonusEnded));
		pinGod.Connect(nameof(PinGodGame.MultiBallEnded), this, nameof(EndMultiball));
		pinGod.Connect(nameof(PinGodGame.ScoreEntryEnded), this, nameof(OnScoreEntryEnded));
        //pinGod.Connect(nameof(PinGodGameBase.PlayerAdded), this, "OnPlayerAdded");

        pinGod.LogInfo("game: creating tilt timeout");
        _tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		AddChild(_tiltedTimeOut);
		_tiltedTimeOut.Connect("timeout", this, nameof(timeout));
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
	public virtual void AddMultiballSceneToTree()
	{
		//create an mball instance from the packed scene
		var mball = multiballPkd.Instance();
		//add to multiball group
		mball.AddToGroup("multiball");
		//add to the tree
		GetNode("Modes").AddChild(mball);
	}
    /// <summary>
    /// add points with 25 bonus
    /// </summary>
    /// <param name="points"></param>
    public virtual void AddPoints(int points)
    {
        pinGod.AddPoints(points);
        pinGod.AddBonus(25);
    }

    /// <summary>
    /// Sets <see cref="PinGodGame.IsMultiballRunning"/> to false and Any node that is in the multiball group is removed from tree
    /// </summary>
    public virtual void EndMultiball()
    {
        pinGod.LogInfo("removing multiballs");
        GetTree().CallGroup("multiball", "EndMultiball");
        pinGod.IsMultiballRunning = false;
    }

    /// <summary>
    /// Add a display at end of ball
    /// </summary>
    public virtual void OnBallEnded(bool lastBall)
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
    public virtual void OnBonusEnded()
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
    /// <summary>
    /// When score entry is finished set <see cref="PinGodGame.EndOfGame"/>
    /// </summary>
    public virtual void OnScoreEntryEnded()
    {
        pinGod.LogInfo(nameof(OnScoreEntryEnded));
        pinGod.EndOfGame();
    }

    /// <summary>
    /// Starts new ball in PinGod and invokes OnBallStarted on all Mode groups
    /// </summary>
    public virtual void StartNewBall()
    {
        pinGod.DisableAllLamps();
        pinGod.StartNewBall();
        pinGod.OnBallStarted(GetTree());
    }

    /// <summary>
    /// Invokes <see cref="PinGodGame.OnBallDrained(SceneTree, string, string)"/> on the whole tree
    /// </summary>
    public virtual void OnBallDrained()
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
    public virtual void OnBallSaved() => pinGod.OnBallSaved(GetTree());

	void OnStartNewBall()
	{
		pinGod.LogInfo("game: starting ball after tilting");
		StartNewBall();
	}
    /// <summary>
    /// Uses Godots <see cref="Godot.Object.CallDeferred(string, object[])"/> to invoke <see cref="OnStartNewBall"/> on Tilt timeouts
    /// </summary>
    void timeout()
    {		
		if (!_lastBall)
		{
			CallDeferred(nameof(OnStartNewBall));
		}
	}
}
