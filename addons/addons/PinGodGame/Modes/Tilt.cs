using Godot;

/// <summary>
/// Listens to tilted actions. "slam_tilt" or "tilt", <see cref="_Input(Godot.InputEvent)"/>
/// </summary>
public class Tilt : Control
{
	/// <summary>
	/// How many warnings a player is allowed before tilting
	/// </summary>
	[Export] byte _num_tilt_warnings = 2;

	private Timer timer;
	float displayForSecs = 2f;
	/// <summary>
	/// singleton
	/// </summary>
	protected PinGodGame pinGod;
	private Trough trough;
	private BlinkingLabel blinkingLayer;

    /// <summary>
    /// Gets access to the game and the trough. Gets the timer and label to show if tilted
    /// </summary>
    public override void _Ready()
	{
		//hide this mode
		Visible = false;

		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		trough = pinGod.GetNodeOrNull<Trough>("Trough");
		//text layer to display warnings and tilted
		blinkingLayer = GetNode("CenterContainer/BlinkingLabel") as BlinkingLabel;
		blinkingLayer.Text = "";

		//timer to hide the tilt layers
		timer = GetNode("Timer") as Timer;		
	}

	/// <summary>
	/// Sends <see cref="PinGodGame.IsTilted"/> if tilted or slam tilt
	/// </summary>
	/// <param name="input"></param>
	public override void _Input(InputEvent input)
	{
		if (!pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return;

		if (pinGod.SwitchOn("tilt", input))
		{
			pinGod.LogInfo("tilt active");
			if (!timer.IsStopped()) { timer.Stop(); }

			//add a warning
			pinGod.Tiltwarnings++;
			//set tilted
			if (pinGod.Tiltwarnings > _num_tilt_warnings)
			{
				pinGod.IsTilted = true;
				pinGod.EnableFlippers(0);
				trough?.DisableBallSave();				
				Visible = true;
				pinGod.LogInfo("Tilt: game tilted");
				ShowTilt();
			}
			//show warnings
			else
			{
				ShowWarning();
			}
		}

		//Slam tilt - Sends 
		if (pinGod.SwitchOn("slam_tilt", input))
		{
			timer.Stop();
            pinGod.LogInfo("Tilt: slam tilt");
            SetText(Tr("SLAMTILT"));
			pinGod.PlaySfx("tilt");
			pinGod.IsTilted = true;			
			pinGod.EnableFlippers(0);
			Visible = true;
			trough?.DisableBallSave();
		}
	}

	void _on_Timer_timeout() => Visible = false;

	/// <summary>
	/// Sets a blinking layer text label using the translate message TILT to languages available
	/// </summary>
	public virtual void ShowTilt()
    {
		pinGod.PlaySfx("tilt");
		//stop the timer for showing tilt information
		CallDeferred(nameof(SetText), Tr("TILT"));
	}

	/// <summary>
	/// Show the player how many tilt warnings
	/// </summary>
	public virtual void ShowWarning()
	{
		timer.Start(displayForSecs);
		pinGod.PlaySfx("warning");
		CallDeferred(nameof(SetText), $"{Tr("TILT_WARN")} {pinGod.Tiltwarnings}");
		Visible = true;
	}

	/// <summary>
	/// Sets text of the BlinkingLayer
	/// </summary>
	/// <param name="text"></param>
	public virtual void SetText(string text) => blinkingLayer.Text = text;

    ///// <summary>
    ///// Hide the tilt "screen" and stop any timers
    ///// </summary>
    void OnBallStarted()
	{
		if (!timer.IsStopped()) { timer.Stop(); }
        SetText("");
		Visible = false;
	}
}
