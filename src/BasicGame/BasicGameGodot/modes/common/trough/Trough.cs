using Godot;
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
	/// Is ball save active
	/// </summary>
	public static bool BallSaveActive { get; internal set; }
	/// <summary>
	/// Ball save time
	/// </summary>
	public static int BallSaveSeconds { get; set; } = 8;
	public static int BallSaveMultiballSeconds { get; set; }

	public static byte NumOfBallToSave { get; set; } = 1;

	/// <summary>
	/// Array of switches for a pinball trough. Used to check if full before starting game
	/// </summary>
	public static readonly byte[] TroughSwitches = new byte[4] { 81, 82, 83, 84 };
	#endregion

	/// <summary>
	/// Ball saver timer. Setup in <see cref="_Ready"/>
	/// </summary>
	private Timer ballSaverTimer;
	private PinGodGame pinGod;
	private Timer troughPulseTimer;

	/// <summary>
	/// init <see cref="ballSaverTimer"/> and connect to <see cref="_timeout"/> to fire when finished.
	/// </summary>
	public override void _Ready()
	{
		ballSaverTimer = new Timer() { OneShot = true, Autostart = false };
		AddChild(ballSaverTimer);
		ballSaverTimer.Connect("timeout", this, "_timeout");

		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		pinGod.Connect("MultiballStarted", this, "OnMultiballStarted");

		Print("trough:loaded");
	}

	/// <summary>
	/// Listen for actions. Mainly from trough switches and plunger lanes.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		//Check the last switch in the Trough Switches array
		if (!pinGod.IsMultiballRunning && pinGod.SwitchOn("trough_4", @event))
		{
			//Put the ball back into play if BallSaveActive
			if (IsTroughFull() && pinGod.GameInPlay)
			{
				Print("trough: is full");
				if (BallSaveActive && !pinGod.IsTilted)
				{
					Print("trough:ball_saved");
					pinGod.SolenoidPulse("trough");
					EmitSignal("BallSaved");
				}
			}
		}

		if (BallSaveActive && !pinGod.IsTilted)
		{
			if (pinGod.SwitchOn("outlane_l", @event) || pinGod.SwitchOn("outlane_r", @event))
			{
				FireEarlySave();
			}
		}

		//when the ball is in the plunger lane stop any ball search running TODO
		if (pinGod.SwitchOn("plunger_lane", @event))
		{
			//auto plunge the ball if in ball save or game is tilted to get the balls back
			if (Trough.BallSaveActive || pinGod.IsTilted)
            {
				pinGod.SolenoidPulse("auto_plunger");
            }
		}
		//reset the ball search when leaving the switch
		else if (pinGod.SwitchOff("plunger_lane", @event))
		{
			//start a ball saver if game in play
			if (pinGod.GameInPlay && !pinGod.IsTilted && !pinGod.IsMultiballRunning)
			{
				var saveStarted = StartBallSaver();
				if (saveStarted)
					EmitSignal(nameof(BallSaveStarted));

				Print($"trough: ball_save_started? {saveStarted}");
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

		var isFull = true;
		for (int i = 0; i < TroughSwitches.Length; i++)
		{
			if (!Input.IsActionPressed("sw" + TroughSwitches[i]))
			{
				isFull = false;
				break;
			}
		}
		return isFull;
	}

	/// <summary>
	/// Counts the number of trough switches currently active
	/// </summary>
	/// <returns></returns>
	public static int BallsInTrough()
	{
		var cnt = 0;
		for (int i = 0; i < TroughSwitches.Length; i++)
		{
			if (Input.IsActionPressed("sw" + TroughSwitches[i]))
			{
				cnt++;
			}
		}
		return cnt;
	}

	public void OnMultiballStarted()
	{		
		BallSaveActive = true;
		OscService.SetLampState(Machine.Lamps["shoot_again"], 2);
		ballSaverTimer.Start(BallSaveMultiballSeconds);
		Print("trough: mball started, save time:", BallSaveMultiballSeconds);

		troughPulseTimer = new Timer() { OneShot = false, Autostart = true, WaitTime = 1, Name = "TroughPulseTimer" };
		troughPulseTimer.Connect("timeout", this, "_trough_pulse_timeout");
		AddChild(troughPulseTimer);		
	}

	void _trough_pulse_timeout()
	{
		if (!Machine.Switches["plunger_lane"].IsOn())
		{
			if (BallsInTrough() < NumOfBallToSave)
			{
				pinGod.SolenoidPulse("trough", 125);
			}
		}

		BallSaveMultiballSeconds--;
		Print("mball sec remain", BallSaveMultiballSeconds);
		if(BallSaveMultiballSeconds < 1)
		{			
			troughPulseTimer.Stop();
			RemoveChild(troughPulseTimer);
			Print("trough: ended mball trough pulse");
			DisableBallSave();
		}
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
			//set to blinking for visual pinball
			OscService.SetLampState(Machine.Lamps["shoot_again"], 2);
			ballSaverTimer.Start(BallSaveSeconds);
		}

		return BallSaveActive;
	}

	public void DisableBallSave()
	{
		BallSaveActive = false;
		OscService.SetLampState(Machine.Lamps["shoot_again"], 0);
	}

	private void FireEarlySave()
	{
		Print("trough:early ball_saved");
		pinGod.SolenoidPulse("trough");
		EmitSignal("BallSaved");
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
