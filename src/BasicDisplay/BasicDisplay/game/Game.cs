using Godot;

public class Game : PinGodGameNode
{
    [Export] protected string MULTIBALL_SCENE = "res://addons/PinGodGame/Modes/multiball/Multiball.tscn";
    private PackedScene _ballSaveScene;
    private bool _lastBall;
    private Timer _tiltedTimeOut;
    [Export] Godot.Collections.Dictionary<byte, string> _vpCommands = new Godot.Collections.Dictionary<byte, string>();
    [Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";
    [Export] int _vpCommandSwitchId = 24;
    private Bonus endOfBallBonus;
    private PackedScene multiballPkd;	
	private ScoreEntry scoreEntry;

	public override void _EnterTree()
	{
		base._EnterTree();

		pinGod.LogInfo("game: enter tree");

		//get packed scene to create an instance of when a Multiball gets activated
		multiballPkd = ResourceLoader.Load(MULTIBALL_SCENE) as PackedScene;
        _ballSaveScene = GD.Load<PackedScene>(BALL_SAVE_SCENE);

        pinGod.Connect(nameof(PinGodGame.BallDrained), this, "OnBallDrained");
		pinGod.Connect(nameof(PinGodGame.BallEnded), this, "OnBallEnded");
		pinGod.Connect(nameof(PinGodGame.BallSaved), this, "OnBallSaved");
		pinGod.Connect(nameof(PinGodGame.BonusEnded), this, "OnBonusEnded");
		pinGod.Connect(nameof(PinGodGame.MultiBallEnded), this, "EndMultiball");
		pinGod.Connect(nameof(PinGodGame.ScoreEntryEnded), this, "OnScoreEntryEnded");
		//pinGod.Connect(nameof(PinGodGameBase.PlayerAdded), this, "OnPlayerAdded");

		//visual pinball command
		pinGod.GameSettings.VpCommandSwitchId = _vpCommandSwitchId;
		pinGod.Connect(nameof(PinGodGame.VpCommand), this, "OnVpCommand");

		scoreEntry = GetNode("Modes/ScoreEntry") as ScoreEntry;
		endOfBallBonus = GetNode("Modes/Bonus") as Bonus;

		_tiltedTimeOut = new Timer() { OneShot = true, WaitTime = 4, Autostart = false };
		AddChild(_tiltedTimeOut);
		_tiltedTimeOut.Connect("timeout", this, "timeout");
	}

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    /// <summary>
    /// Basic input switch handling
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (!pinGod.GameInPlay) return;
        if (pinGod.IsTilted) return; //game is tilted, don't process other switches when tilted

        if (pinGod.SwitchOn("start", @event))
        {
            pinGod.LogInfo("base: start button adding player...", pinGod.StartGame());
        }
        if (pinGod.SwitchOn("flipper_l", @event))
        {
        }
        if (pinGod.SwitchOn("flipper_r", @event))
        {
        }
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

    void ClearScene(Control ctl)
    {
        pinGod.LogInfo("clearing scene");
        if (ctl != null)
        {
            pinGod.LogInfo("array good");
            ctl.Visible = false;
        }
    }

    /// <summary>
    /// Adds a ball save scene to the tree and removes
    /// </summary>
    /// <param name="time">removes the scene after the time</param>
    private void DisplayBallSaveScene(float time = 2f)
    {
        var ballSaveScene = _ballSaveScene.Instance<BallSave>();
        ballSaveScene.SetRemoveAfterTime(time);
        GetNode<CanvasLayer>("Modes").AddChild(ballSaveScene);
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
    void OnBallSaved() 
    {
        pinGod.LogDebug("base: ball_saved");
        if (!pinGod.IsMultiballRunning)
        {
            pinGod.LogDebug("ballsave: ball_saved");
            //add ball save scene to tree and remove after 2 secs;
            CallDeferred(nameof(DisplayBallSaveScene), 2f);
        }
        else
        {
            pinGod.LogDebug("skipping save display in multiball");
        }

        pinGod.OnBallSaved(GetTree());
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
        StartNewBall();
    }

    /// <summary>
    /// Signal sent from PinGod when VPCommand Switch is found
    /// </summary>
    /// <param name="index"></param>
    void OnVpCommand(byte index)
	{
        if (_vpCommands.ContainsKey(index))
        {
            pinGod.LogDebug("vp_command: " + _vpCommands[index]);
            var cmd = _vpCommands[index]; //get string command
            var cmds = cmd.Split(",");

            switch (cmds[0])
            {
                case "add_points":
                    if(cmds.Length == 2)
                    {
                        long.TryParse(cmds[1], out var points);
                        if (points > 0)
                            pinGod.AddPoints(points);
                    }
                    break;
                case "multiball":
                    if (cmds.Length == 2)
                    {
                        byte.TryParse(cmds[1], out var balls);
                        if (balls > 0)
                            pinGod.StartMultiBall(balls);
                    }
                    break;
                case "scene":
                    if (cmds.Length >= 2)
                    {
                        var scene = GetNodeOrNull<Control>($"Modes/{cmds[1]}");
                        if (scene != null)
                        {
                            var timer = scene.GetNodeOrNull<Timer>("Timer");
                            if(timer != null)
                            {
                                if (!timer.IsConnected("timeout", this, "ClearScene"))
                                {
                                    timer.Connect("timeout", this, "ClearScene", new Godot.Collections.Array() { scene });
                                }

                                float time = 2f;
                                if(cmds.Length == 3)
                                {
                                    float.TryParse(cmds[2], out time);
                                }
                                timer.Start(time);
                                scene.Visible = true;
                            }                            
                        }                            
                    }
                    break;
                default:
                    break;
            }
        }
		//switch (index)
		//{
		//	case 1:
		//		pinGod.AddPoints(1000);
		//		break;
		//	case 2:
		//		pinGod.AddPoints(5000);
		//		break;
		//	case 3:
		//		pinGod.AddBonus(500);
		//		break;
		//	case 254:
		//		pinGod.StartNewBall();
		//		break;
		//	case 255:
  //              OnBallDrained(); //will call EndBall and let any modes know player has drained
  //              break;
		//	default:
		//		break;
		//}
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
