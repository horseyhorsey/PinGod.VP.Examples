using Godot;

public class BaseGameMode : Control
{
    public const int BASIC_SWITCH_VALUE = 10000;
    private PackedScene _ballSaveScene;
    private Timer _clearLayersTimer;
    VideoStreamTheora[] _deathScenes = new VideoStreamTheora[3];
    VideoStreamTheora[] _hoopScenes = new VideoStreamTheora[3];
    VideoStreamTheora _kickbackScene = null;
    private Label _label;
    private RandomNumberGenerator _rng;
    private VideoPlayerPinball _videoPlayer;
    [Export] string BALL_SAVE_SCENE = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";
    private Game Game;
	private JawsPinGodGame pinGod;

	//hoop drains
	public override void _EnterTree()
    {
        pinGod = GetNode("/root/PinGodGame") as JawsPinGodGame;
        Game = GetParent().GetParent() as Game;

        _videoPlayer = GetNode("VideoPlayerPinball") as VideoPlayerPinball;
        _clearLayersTimer = GetNode("ClearLayersTimer") as Timer;
        _label = GetNode("Label") as Label;

        _ballSaveScene = GD.Load<PackedScene>(BALL_SAVE_SCENE);

        GetResources();
    }

    public override void _Input(InputEvent @event)
    {
        if (!pinGod.GameInPlay || pinGod.IsTilted) return;

        if (pinGod.SwitchOn("outlane_l", @event))
        {
            Game.AddPoints(BASIC_SWITCH_VALUE);
            if (Game.KickbackEnabled)
            {
                pinGod.SolenoidPulseTimer("kickback");
                pinGod.SetLampState("kickback", 2);
                Game.EnableKickback(false);
                CallDeferred(nameof(PlayKickbackScene));
            }
            else
            {
                if (!pinGod.BallSaveActive && !pinGod.IsMultiballRunning)
                    CallDeferred(nameof(RandomBallDrain));
            }
        }
        if (pinGod.SwitchOn("inlane_l", @event))
        {
            Game.AddPoints(BASIC_SWITCH_VALUE);
            if (!pinGod.IsMultiballRunning)
            {
                pinGod.SolenoidOn("vpcoil", 0);//stop any lampshows
            }
        }
        if (pinGod.SwitchOn("inlane_r", @event))
        {
            Game.AddPoints(BASIC_SWITCH_VALUE);            
        }
        if (pinGod.SwitchOn("outlane_r", @event))
        {
            Game.AddPoints(BASIC_SWITCH_VALUE);

            if (!pinGod.BallSaveActive && !pinGod.IsMultiballRunning)
                CallDeferred(nameof(RandomHoopScene));
        }
    }

    public override void _Ready()
    {
        _rng = new Godot.RandomNumberGenerator();
        _rng.Randomize();

        //Tests
        //RandomBallDrain();
        //RandomHoopScene();
        //PlayKickbackScene();
    }

    public void OnBallSaved()
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
    }

    public void OnBallStarted()
    {
        pinGod.LogInfo("base: ball started");

        Game.currentPlayer = pinGod.GetJawsPlayer();
        Game.DisableBarrelTargets();
        Game.UpdateLamps();
        Game.CallDeferred("UpdateProgress");

        Game.PlayMusicForMode();
    }

    public void UpdateLamps()
    {
        pinGod.SetLampState("gi_0", 1);
        pinGod.SetLampState("gi_1", 1);
        pinGod.SetLampState("gi_2", 1);

        if (Game.KickbackEnabled) { pinGod.SetLampState("kickback", 1); }
        else pinGod.SetLampState("kickback", 0);
    }

    private void _on_ClearLayersTimer_timeout()
    {
        ShowLayers(false);
    }

    private void _on_VideoPlayerPinball_finished()
    {
        _clearLayersTimer.Start();
    }

    /// <summary>
    /// Adds a ball save scene to the tree and removes
    /// </summary>
    /// <param name="time">removes the scene after the time</param>
    private void DisplayBallSaveScene(float time = 2f)
    {
        var ballSaveScene = _ballSaveScene.Instance<BallSave>();
        ballSaveScene.SetRemoveAfterTime(time);
        AddChild(_ballSaveScene.Instance());
    }

    private void GetResources()
    {
        var _resources = pinGod.GetResources();
        _kickbackScene = _resources.Resolve("jaws_jump") as VideoStreamTheora;
        _deathScenes[0] = _resources.Resolve("death_boy") as VideoStreamTheora;
        _deathScenes[1] = _resources.Resolve("death_chrissy") as VideoStreamTheora;
        _deathScenes[2] = _resources.Resolve("death_leg") as VideoStreamTheora;
        _hoopScenes[0] = _resources.Resolve("hoop_arse") as VideoStreamTheora;
        _hoopScenes[1] = _resources.Resolve("hoop_fist") as VideoStreamTheora;
        _hoopScenes[2] = _resources.Resolve("hoop_letit_go") as VideoStreamTheora;
    }
	/// <summary>
	/// only plays when not in multiball
	/// </summary>
	void PlayKickbackScene()
	{
		_clearLayersTimer.Stop();
		if (!pinGod.IsMultiballRunning)
		{
			_label.Text = Tr("KICKBACK");
			_videoPlayer.Stream = _kickbackScene;
			_videoPlayer.Play();
			ShowLayers(true);
		}				
	}

	void RandomBallDrain()
	{
		_clearLayersTimer.Stop();
		var index = _rng.RandiRange(0, _deathScenes.Length - 1);
		pinGod.LogInfo("death scene: ", index);
		_label.Text = Tr("BALL_EATEN");
		_videoPlayer.Stream = _deathScenes[index];
		_videoPlayer.Play();
		ShowLayers(true);
	}

	void RandomHoopScene()
	{
		_clearLayersTimer.Stop();
		_label.Text = Tr("BALL_LOST");
		var index = _rng.RandiRange(0, _hoopScenes.Length - 1);
		pinGod.LogInfo("hoop scene: ", index);
		_videoPlayer.Stream = _hoopScenes[index];
		_videoPlayer.Play();
		ShowLayers(true);
	}

	private void ShowLayers(bool show)
	{
		_videoPlayer.Visible = show;
		_label.Visible = show;
		this.Visible = show;
	}
}
