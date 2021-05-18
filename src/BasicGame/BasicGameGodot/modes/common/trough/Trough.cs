using Godot;
using System;
using static Godot.GD;

/// <summary>
/// Simple simulation of a Pinball ball trough. Handles trough switches, ball saves <para/>
/// Loaded as singleton on launch. Visual pinball uses the `bsTrough` in example
/// </summary>
public class Trough : Node
{
	/// <summary>
	/// Use this to turn off trough checking, outside VP
	/// </summary>
	public static bool _isDebugTrough = false;

	#region Signals
	/// <summary>
	/// Ball just saved
	/// </summary>
	[Signal] public delegate void BallSaved();
	[Signal] public delegate void BallSaveStarted();
	[Signal] public delegate void BallSaveEnded(); 
	#endregion

	#region Public Static
	/// <summary>
	/// Lamp number to blink when ball save is active
	/// </summary>
	public static byte BallSaveLamp { get; private set; } = 1;
	/// <summary>
	/// Is ball save active
	/// </summary>
	public static bool BallSaveActive { get; internal set; }
	/// <summary>
	/// Ball save time
	/// </summary>
	public static int BallSaveSeconds { get; set; } = 8;
	/// <summary>
	/// Array of switches for a pinball trough. Used to check if full before starting game
	/// </summary>
	public static readonly byte[] TroughSwitches = new byte[4] { 81, 82, 83, 84 }; 
	#endregion

	/// <summary>
	/// The switch to check when released to activate ball saves
	/// </summary>
	static byte PlungerLaneSwitch = 20;
	/// <summary>
	/// Ball saver timer. Setup in <see cref="_Ready"/>
	/// </summary>
	private Timer ballSaverTimer;

	/// <summary>
	/// init <see cref="ballSaverTimer"/> and connect to <see cref="_timeout"/> to fire when finished.
	/// </summary>
	public override void _Ready()
	{
		ballSaverTimer = new Timer() { OneShot = true, Autostart = false };
		AddChild(ballSaverTimer);
		ballSaverTimer.Connect("timeout", this, "_timeout");

		var globals = GetNode("/root/GameGlobals") as GameGlobals;
		globals.Connect("GameTilted", this, "OnGameTilted");

		Print("trough:loaded");
	}

	/// <summary>
	/// Listen for actions. Mainly from trough switches and plunger lanes.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		//Check the last switch in the Trough Switches array
		if (@event.IsActionPressed("sw" + TroughSwitches[TroughSwitches.Length - 1]))
		{
			Print("trough: last switch");
			//Put the ball back into play if BallSaveActive
			if (IsTroughFull() && GameGlobals.GameInPlay)
			{
				if (BallSaveActive)
				{
					Print("trough:ball_saved");
					EmitSignal("BallSaved");
					OscService.PulseCoilState(GameGlobals.TroughSolenoid);
				}
			}
		}

		if (@event.IsActionPressed($"sw{PlungerLaneSwitch}"))
		{
			Print("trough:entered plunger lane");
		}
		else if (@event.IsActionReleased($"sw{PlungerLaneSwitch}"))
		{
			if (GameGlobals.GameInPlay && !GameGlobals.IsTilted)
			{
				var saveStarted = StartBallSaver();
				if (saveStarted)
					EmitSignal(nameof(BallSaveStarted));

				Print($"trough:left plunger lane: Ball save started? {saveStarted}");
			}
		}
	}

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
	/// Activates the ball saver if not already running. Blinks the ball saver lamp
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

	/// <summary>
	/// Handler for <see cref="GameGlobals.GameTilted"/> to disable any ball saves
	/// </summary>
	public void OnGameTilted()
	{
		Print("trough:game_tilted");
		GameGlobals.GameData.Tilted++;
		DisableBallSave();
	}

	public static void DisableBallSave()
	{
		BallSaveActive = false;
		OscService.SetLampState(BallSaveLamp, 0);
	}

	/// <summary>
	/// Disable ball saver and lamps
	/// </summary>
	private void _timeout()
	{
		DisableBallSave();
		EmitSignal(nameof(BallSaveEnded));
	}
}
