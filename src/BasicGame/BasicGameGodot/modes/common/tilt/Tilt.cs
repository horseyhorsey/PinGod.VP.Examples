using Godot;
using static Godot.GD;

/// <summary>
/// Manages the <see cref="GameGlobals.TiltSwitchNum"/> and <see cref="GameGlobals.SlamTiltSwitch"/> inputs <para/>
/// Sets the <see cref="GameGlobals.Tiltwarnings"/> and <see cref="GameGlobals.IsTilted"/> if player goes over their warnings
/// </summary>
public class Tilt : Control
{
	/// <summary>
	/// How many warnings a player is allowed before tilting
	/// </summary>
	[Export] byte _num_tilt_warnings = 2;

	private Timer timer;
	float displayForSecs = 2f;
	private GameGlobals gGlobals;
	private TextLayer blinkingLayer;

	public override void _Ready()
	{
		//hide this mode
		Visible = false;

		gGlobals = GetNode("/root/GameGlobals") as GameGlobals;
		//text layer to display warnings and tilted
		blinkingLayer = GetNode("TextLayerControl") as TextLayer;
		blinkingLayer.SetText("");

		//reset the tilt if new ball started
		gGlobals.Connect("BallStarted", this, "OnBallStarted");

		//timer to hide the tilt layers
		timer = GetNode("Timer") as Timer;		
	}

	/// <summary>
	/// Sends <see cref="GameGlobals.GameTilted"/> if tilted or slam tilt
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("sw" + GameGlobals.TiltSwitchNum))
		{
			if (!GameGlobals.IsTilted && !Trough.IsTroughFull())
			{				
				//add a warning
				GameGlobals.Tiltwarnings++;
				timer?.Stop();
				//set tilted
				if (GameGlobals.Tiltwarnings > _num_tilt_warnings)
				{
					//stop the timer for showing tilt information
					GameGlobals.IsTilted = true;
					Print("game tilted");
					blinkingLayer.SetText("Tilted");
					Visible = true;
					gGlobals.EnableFlippers(0);
					gGlobals.EmitSignal("GameTilted");					
				}
				//show warnings
				else
				{
					ShowWarning();
					Visible = true;
				}
			}            
		}

		//Slam tilt - Sends 
		if (@event.IsActionPressed("sw" + GameGlobals.SlamTiltSwitch))
		{
			timer?.Stop();
			Print("slam tilt");
			blinkingLayer.SetText("SLAM TILT");
			GameGlobals.IsTilted = true;
			gGlobals.EmitSignal("GameTilted");
			gGlobals.EnableFlippers(0);
			Visible = true;
		}
	}

	void _on_Timer_timeout() => Visible = false;

	/// <summary>
	/// Show the player how many tilt warnings
	/// </summary>
	void ShowWarning()
	{        		
		Print("tilt warning: " + GameGlobals.Tiltwarnings);
		blinkingLayer.SetText("Danger" + System.Environment.NewLine + $"Warnings {GameGlobals.Tiltwarnings}");
		timer?.Start(displayForSecs);
	}

	/// <summary>
	/// Hide the tilt "screen" and stop any timers
	/// </summary>
	void OnBallStarted()
	{
		this.Visible = false;
		timer?.Stop();
	}
}
