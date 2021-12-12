using Godot;

public class Game : PinGodGameNode
{
	[Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";

	private bool _lastBall;	
	private Timer _tiltedTimeOut;
	private Bonus endOfBallBonus;
    private Node2D displayMsgContainer;
    private PackedScene multiballPkd;
	private ScoreEntry scoreEntry;
    private Control scoreNode;

    #region Nightmare property add on
    private PackedScene _midnightModePacked;
	public float MusicPauseTime { get; set; }
	public bool RemixSoundsMode { get; internal set; }	
	public Timer resumeBgmTimer { get; private set; }

    private Timer displayTimer;
    private Label nightmareMsg;
	#endregion

	public override void _EnterTree()
	{
		base._EnterTree();
		pinGod.LogInfo("game: enter tree");

		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;

		_midnightModePacked = ResourceLoader.Load("res://modes/custom/Midnight.tscn") as PackedScene;
		
		pinGod.Connect(nameof(PinGodGameBase.BallDrained), this, "OnBallDrained");
		pinGod.Connect(nameof(PinGodGameBase.BallEnded), this, "OnBallEnded");
		pinGod.Connect(nameof(PinGodGameBase.BonusEnded), this, "OnBonusEnded");
		pinGod.Connect(nameof(PinGodGameBase.MultiBallEnded), this, "EndMultiball");
		pinGod.Connect(nameof(PinGodGameBase.ScoreEntryEnded), this, "OnScoreEntryEnded");

		scoreEntry = GetNode("Modes/ScoreEntry") as ScoreEntry;
		endOfBallBonus = GetNode("Modes/Bonus") as Bonus;

		_tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		AddChild(_tiltedTimeOut);
		_tiltedTimeOut.Connect("timeout", this, "timeout");

		resumeBgmTimer = GetNode<Timer>("ResumeMusicTimer");

		displayTimer = GetNode<Timer>("DisplayMessageTimer");
		displayTimer.Connect("timeout", this, "OnDisplayTimeout");
		displayMsgContainer = GetNode("Modes/Node2D") as Node2D;
		nightmareMsg = GetNode<Label>("Modes/Node2D/NightmareMessage");
		displayMsgContainer.Visible = false;
	}
	public override void _Ready()
	{
		pinGod.DisableAllLamps();
		pinGod.LogInfo("game: _ready");		
		pinGod.BallInPlay = 1;
		pinGod.StartNewBall();		
		pinGod.OnBallStarted(GetTree());
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
			endOfBallBonus.CallDeferred("StartBonusDisplay");
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
			pinGod.StartNewBall();
			pinGod.OnBallStarted(GetTree());
			pinGod.UpdateLamps(GetTree());
		}
	}

	public void OnDisplayMessage(string message, float time = 1.5f)
    {
		displayTimer.Stop();
		displayTimer.Start(time);
		displayMsgContainer.Visible = true;
		nightmareMsg.Text = message;		

		pinGod.LogInfo("display message");
    }

	void OnDisplayTimeout()
    {
		displayMsgContainer.Visible = false;
	}

	/// <summary>
	/// Stops the music, saves the time ...plays new music sound and resumes after
	/// </summary>
	/// <param name="music"></param>
	/// <param name="resumeDelay"></param>	
	internal void PlayThenResumeMusic(string music, float resumeDelay)
	{
		resumeBgmTimer.Stop();

		//only save the pause time if main music playing
		if (pinGod.AudioManager.CurrentMusic == "mus_main")
        {
			MusicPauseTime = pinGod.StopMusic();
			pinGod.LogDebug("main music paused at ", MusicPauseTime);
		}			
		else pinGod.StopMusic();

		pinGod.PlayMusic(music);
		resumeBgmTimer.Start(resumeDelay);
	}
	internal void StartMidnight()
	{
		GetNode("Modes").AddChild(_midnightModePacked.Instance());
	}
	private void _on_ResumeMusicTimer_timeout()
	{
		var p = pinGod.Player as NightmarePlayer;
        if (p.IsRemixMode) pinGod.PlayMusic("mus_remix", MusicPauseTime);
        else pinGod.PlayMusic("mus_main", MusicPauseTime);
    }
	void AddMultiballSceneToTree()
	{
		//create an mball instance from the packed scene
		var mball = multiballPkd.Instance();
		//add to multiball group
		mball.AddToGroup("multiball");
		//add to the tree
		GetNode("Modes").AddChild(mball);
	}
	private void AddPoints(int points)
	{
		pinGod.AddPoints(points);
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
	/// When score entry is finished set <see cref="PinGodGame.EndOfGame"/>
	/// </summary>
	void OnScoreEntryEnded()
	{
		pinGod.EndOfGame();
	}
	void OnStartNewBall()
	{
		pinGod.LogInfo("game: starting ball after tilting");
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
