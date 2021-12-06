using Godot;

/// <summary>
/// Manages the <see cref="PinGodGame.TiltSwitchNum"/> and <see cref="PinGodGame.SlamTiltSwitch"/> inputs <para/>
/// Sets the <see cref="PinGodGame.Tiltwarnings"/> and <see cref="PinGodGame.IsTilted"/> if player goes over their warnings
/// </summary>
public class Tilt : Control
{
	/// <summary>
	/// How many warnings a player is allowed before tilting
	/// </summary>
	[Export] byte _num_tilt_warnings = 2;

	private Timer timer;
	float displayForSecs = 2f;
	private PinGodGame pinGod;
	private Trough trough;
	private BlinkingLabel blinkingLayer;

	public override void _Ready()
	{
		//hide this mode
		Visible = false;

		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		trough = GetNode("/root/Trough") as Trough;
		//text layer to display warnings and tilted
		blinkingLayer = GetNode("CenterContainer/BlinkingLabel") as BlinkingLabel;
		blinkingLayer.Text = "";

		//timer to hide the tilt layers
		timer = GetNode("Timer") as Timer;		
	}

	/// <summary>
	/// Sends <see cref="PinGodGame.GameTilted"/> if tilted or slam tilt
	/// </summary>
	/// <param name="input"></param>
	public override void _Input(InputEvent input)
	{
		if (!pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return;

		if (pinGod.SwitchOn("tilt", input))
		{
			if (!timer.IsStopped()) { timer.Stop(); }

			//add a warning
			pinGod.Tiltwarnings++;
			//set tilted
			if (pinGod.Tiltwarnings > _num_tilt_warnings)
			{
				pinGod.IsTilted = true;
				pinGod.EnableFlippers(0);
				trough.DisableBallSave();

				Visible = true;
				pinGod.LogInfo("game tilted");
				//stop the timer for showing tilt information
				CallDeferred("setText", "TILTED");
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
			pinGod.LogInfo("slam_tilt");
			setText("SLAM TILT");
			pinGod.IsTilted = true;			
			pinGod.EnableFlippers(0);
			Visible = true;
			trough.DisableBallSave();
		}
	}

	void _on_Timer_timeout() => Visible = false;

	/// <summary>
	/// Show the player how many tilt warnings
	/// </summary>
	void ShowWarning()
	{
		timer.Start(displayForSecs);
		CallDeferred("setText", "Danger" + System.Environment.NewLine + $"Warnings {pinGod.Tiltwarnings}");
		Visible = true;
	}

	void setText(string text)
	{
		blinkingLayer.Text = text;
	}

	///// <summary>
	///// Hide the tilt "screen" and stop any timers
	///// </summary>
	void OnBallStarted()
	{
		if (!timer.IsStopped()) { timer.Stop(); }
		setText("");
		Visible = false;
	}
}
