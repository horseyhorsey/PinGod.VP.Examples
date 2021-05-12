using Godot;
using System;
using static Godot.GD;

/// <summary>
/// Loaded as singleton on launch
/// </summary>
public class Trough : Node
{	
	#region Public Properties
	public static bool BallSaveActive { get; private set; }
	public static int BallSaveSeconds { get; set; } = 10;
	#endregion

	/// <summary>
	/// Ball saver timer. Setup in <see cref="_Ready"/>
	/// </summary>
	private Timer ballSaverTimer;

	/// <summary>
	/// Disable ball saver and lamps
	/// </summary>
	private void _timeout()
	{
		BallSaveActive = false;
		OscService.SetLampState(1, 0);
	}

	/// <summary>
	/// Trough switches to check is full
	/// </summary>
	public static bool IsTroughFull => Input.IsActionPressed("sw81") && Input.IsActionPressed("sw82") && Input.IsActionPressed("sw83") && Input.IsActionPressed("sw84");

	public bool StartBallSaver()
	{
		if (!BallSaveActive)
		{
			BallSaveActive = true;
			OscService.SetLampState(1, 2);
			ballSaverTimer.Start(BallSaveSeconds);
		}

		return BallSaveActive;
	}

	#region Godot Overrides
	/// <summary>
	/// init <see cref="ballSaverTimer"/> and connect to <see cref="_timeout"/> to fire when finished.
	/// </summary>
	public override void _Ready()
	{
		ballSaverTimer = new Timer() { OneShot = true, Autostart = false };
		AddChild(ballSaverTimer);
		ballSaverTimer.Connect("timeout", this, "_timeout");
	}

	/// <summary>
	/// Listen for actions. Mainly from trough switches and plungers.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{        
		if (@event.IsActionPressed("sw84")) //trough entered
		{
			//Put the ball back into play if BallSaveActive
			if (IsTroughFull)
			{
				if (BallSaveActive)
				{
					Print("ball saved!");
					OscService.PulseCoilState(1);                    
				}
			}
		}

		if (@event.IsActionPressed("sw20"))
		{
			Print("sw20:plunger:1");
		}
		else if (@event.IsActionReleased("sw20"))
		{
			Print("sw20:plunger:0");
			StartBallSaver();
		}
	} 
	#endregion
}
