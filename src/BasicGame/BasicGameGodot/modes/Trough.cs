using Godot;
using System;
using static Godot.GD;

/// <summary>
/// Loaded as singleton on launch. Visual pinball uses the `bsTrough`
/// </summary>
public class Trough : Node
{	
	#region Public Properties
	public static byte BallSaveLamp { get; private set; } = 1; // Lamp ID to blink when ball save is active
	public static bool BallSaveActive { get; private set; }    // Is ball save active?
	public static int BallSaveSeconds { get; set; } = 8;      // Ball save time
	static byte[] TroughSwitches = new byte[4] {81, 82, 83, 84}; // Used to check if full before starting game
	static byte PlungerLaneSwitch = 20; // The switch to check when released to activate ball saves
	static byte TroughSolenoid = 1; // The coil to send to Visual Pinball to exit the Trough
	#endregion

	/// <summary>
	/// Ball saver timer. Setup in <see cref="_Ready"/>
	/// </summary>
	private Timer ballSaverTimer;

	/// <summary>
	/// Trough switches to check is full
	/// </summary>
	public static bool IsTroughFull()
	{
		for (int i = 0; i < TroughSwitches.Length; i++)
		{
			if(!Input.IsActionPressed("sw"+TroughSwitches[i]))
				return false;
		}
		
		return true;
	}

	public bool StartBallSaver()
	{
		if (!BallSaveActive)
		{
			BallSaveActive = true;
			OscService.SetLampState(BallSaveLamp, 2);
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
	/// Disable ball saver and lamps
	/// </summary>
	private void _timeout()
	{
		BallSaveActive = false;
		OscService.SetLampState(BallSaveLamp, 0);
	}

	/// <summary>
	/// Listen for actions. Mainly from trough switches and plunger lanes.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{        
		//Check the last switch in the Trough Switches array
		if (@event.IsActionPressed("sw"+TroughSwitches[TroughSwitches.Length-1]))
		{
			//Put the ball back into play if BallSaveActive
			if (IsTroughFull())
			{
				if (BallSaveActive)
				{
					Print("ball saved!");
					OscService.PulseCoilState(TroughSolenoid);                    
				}
			}
		}

		if (@event.IsActionPressed($"sw{PlungerLaneSwitch}"))
		{
			Print("entered plunger lane");
		}
		else if (@event.IsActionReleased($"sw{PlungerLaneSwitch}"))
		{
			Print($"left plunger lane: Ball save started? {StartBallSaver()}");
		}
	} 
	#endregion
}
