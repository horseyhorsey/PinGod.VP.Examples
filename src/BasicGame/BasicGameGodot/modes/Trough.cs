using Godot;
using System;
using static Godot.GD;

/// <summary>
/// Loaded as singleton on launch. Visual pinball uses the `bsTrough`
/// </summary>
public class Trough : Node
{

	[Signal] public delegate void BallSaved();
	[Signal] public delegate void BallSaveStarted();
	[Signal] public delegate void BallSaveEnded();

	/// <summary>
	/// Use this to turn off trough checking, outside VP
	/// </summary>
	static bool _isDebugTrough = false;

	#region Public Properties
	public static byte BallSaveLamp { get; private set; } = 1; // Lamp ID to blink when ball save is active
	public static bool BallSaveActive { get; private set; }    // Is ball save active?
	public static int BallSaveSeconds { get; set; } = 8;      // Ball save time
	public static readonly byte[] TroughSwitches = new byte[4] {81, 82, 83, 84}; // Used to check if full before starting game
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
		if (_isDebugTrough)
			return true;

		for (int i = 0; i < TroughSwitches.Length; i++)
		{
			if(!Input.IsActionPressed("sw"+TroughSwitches[i]))
				return false;
		}
		
		return true;
	}

	/// <summary>
	/// Activates the ball saver if not already running
	/// </summary>
	/// <returns>True if the ball saver is active</returns>
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
		EmitSignal(nameof(BallSaveEnded));
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
					Print("trough:ball_saved");
					EmitSignal("BallSaved");
					OscService.PulseCoilState(TroughSolenoid);        
				}
			}
		}

		if (@event.IsActionPressed($"sw{PlungerLaneSwitch}"))
		{
			Print("trough:entered plunger lane");
		}
		else if (@event.IsActionReleased($"sw{PlungerLaneSwitch}"))
		{
			var saveStarted = StartBallSaver();
			if (saveStarted)
				EmitSignal(nameof(BallSaveStarted));

			Print($"trough:left plunger lane: Ball save started? {saveStarted}");
		}
	} 
	#endregion
}
