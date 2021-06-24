using Godot;
using System.Drawing;
using System.Threading.Tasks;
using static Godot.GD;

/// <summary>
/// Simple simulation of a Pinball ball trough. Handles trough switches, ball saves <para/>
/// Loaded as singleton on launch. Visual pinball uses the `bsTrough` in example.
/// Emits <see cref="PinGodGameBase"/> signals for ball saves, ball end
/// </summary>
public class Trough : Node
{
	/// <summary>
	/// Use this to turn off trough checking, outside VP
	/// </summary>
	public static bool _isDebugTrough = false;
	/// <summary>
	/// Ball saver timer. Setup in <see cref="_Ready"/>
	/// </summary>
	private Timer ballSaverTimer;
	private PinGodGameBase pinGod;
	private Timer troughPulseTimer;
	public TroughOptions TroughOptions { get; set; }

	private int _mballSaveSecondsRemaining;
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGameBase;
		ballSaverTimer = GetNode("BallSaverTimer") as Timer;
		troughPulseTimer = GetNode("TroughPulseTimer") as Timer;
		pinGod.LogInfo("trough:_enter tree");
	}
	/// <summary>
	/// Listen for actions. Mainly from trough switches and plunger lanes.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		//fire early saves
		if (pinGod.IsBallStarted && !pinGod.IsTilted && pinGod.BallSaveActive)
		{
			for (int i = 0; i < TroughOptions?.EarlySaveSwitches?.Length; i++)
			{
				if (pinGod.SwitchOn(TroughOptions.EarlySaveSwitches[i], @event))
				{
					FireEarlySave();
					pinGod.EmitSignal(nameof(PinGodGameBase.BallSaved));
					break;
				}
			}
		}

		//last switch (entry switch)
		if (IsTroughSwitchOn(@event))
		{
			var troughFull = IsTroughFull();
			if (troughFull && pinGod.IsBallStarted && !pinGod.IsMultiballRunning)
			{
				if (pinGod.BallSaveActive && !pinGod.IsTilted)
				{
					pinGod.LogDebug("trough: ball_saved");
					pinGod.SolenoidPulse(TroughOptions.Coil);
					pinGod.EmitSignal(nameof(PinGodGameBase.BallSaved));
				}
				else
				{
					pinGod.EmitSignal(nameof(PinGodGameBase.BallDrained));
				}
			}
			else if (pinGod.IsMultiballRunning && !pinGod.BallSaveActive)
			{
				var balls = BallsInTrough();
				if (TroughOptions.Switches.Length - 1 == balls)
				{
					pinGod.IsMultiballRunning = false;
					troughPulseTimer.Stop();
					pinGod.EmitSignal(nameof(PinGodGameBase.MultiBallEnded));
				}
			}
		}
		if (IsTroughSwitchOff(@event)) { } //to record the switch off

		if (!pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return;

		//when the ball is in the plunger lane stop any ball search running TODO
		if (pinGod.SwitchOn(TroughOptions.PlungerLaneSw, @event))
		{
			//auto plunge the ball if in ball save or game is tilted to get the balls back
			if (pinGod.BallSaveActive || pinGod.IsTilted || pinGod.IsMultiballRunning)
			{
				pinGod.SolenoidPulse(TroughOptions.AutoPlungerCoil, 125);
				pinGod.LogDebug("trough: auto saved");
			}
		}

		//reset the ball search when leaving the switch
		if (pinGod.SwitchOff(TroughOptions.PlungerLaneSw, @event))
		{
			//start a ball saver if game in play
			if (pinGod.GameInPlay && !pinGod.IsTilted && !pinGod.IsMultiballRunning)
			{
				var saveStarted = StartBallSaver(TroughOptions.BallSaveSeconds);
				if (saveStarted)
				{
					UpdateLamps(LightState.Blink);
					pinGod.EmitSignal(nameof(PinGodGameBase.BallSaveStarted));
				}

				pinGod.LogDebug($"trough: ball_save_started? {saveStarted}");
			}
		}
	}
	public override void _Ready()
	{
		pinGod.LogDebug("trough:_ready. switch_count: ", TroughOptions?.Switches.Length);
	}
	public int BallsInPlay() => TroughOptions.Switches.Length - BallsInTrough();
	/// <summary>
	/// Counts the number of trough switches currently active
	/// </summary>
	/// <returns></returns>
	public int BallsInTrough()
	{
		var cnt = 0;
		for (int i = 0; i < TroughOptions.Switches.Length; i++)
		{
			if (pinGod.SwitchOn(TroughOptions.Switches[i]))
			{
				cnt++;
			}
		}
		return cnt;
	}
	public void DisableBallSave()
	{
		pinGod.BallSaveActive = false;
		UpdateLamps(LightState.Off);
	}
	/// <summary>
	/// Trough switches to check is full
	/// </summary>
	public bool IsTroughFull()
	{
		if (_isDebugTrough)
			return true;

		var isFull = true;
		for (int i = 0; i < TroughOptions.Switches.Length; i++)
		{
			if (!pinGod.SwitchOn(TroughOptions.Switches[i]))
			{
				isFull = false;
				break;
			}
		}
		return isFull;
	}
	public void PulseTrough()
	{
		pinGod.SolenoidPulse(TroughOptions.Coil);
	}
	/// <summary>
	/// Activates the ball saver if not already running. Blinks the ball saver lamp
	/// </summary>
	/// <returns>True if the ball saver is active</returns>
	public bool StartBallSaver(float seconds = 8)
	{
		ballSaverTimer.Stop();
		pinGod.BallSaveActive = true;
		ballSaverTimer.Start(seconds);
		UpdateLamps(LightState.Blink);
		return pinGod.BallSaveActive;
	}
	/// <summary>
	/// Starts multi-ball trough
	/// </summary>
	/// <param name="numOfBalls"></param>
	/// <param name="ballSaveTime"></param>
	/// <param name="pulseTimerDelay">Timer to pulse trough</param>
	public void StartMultiball(byte numOfBalls, byte ballSaveTime, float pulseTimerDelay = 1)
	{
		TroughOptions.MballSaveSeconds = ballSaveTime;
		TroughOptions.NumBallsToSave = numOfBalls;

		if (pulseTimerDelay > 0)
			_startMballTrough(pulseTimerDelay);

		OnMultiballStarted();
	}
	void _startMballTrough(float delay) => troughPulseTimer.Start(delay);
	private void FireEarlySave()
	{
		pinGod.LogDebug("trough:early ball_saved");
		PulseTrough();
		pinGod.EmitSignal(nameof(PinGodGameBase.BallSaved));
	}
	bool IsTroughSwitchOn(InputEvent input)
	{
		for (int i = 0; i < TroughOptions.Switches.Length; i++)
		{
			if (pinGod.SwitchOn(TroughOptions.Switches[i], input))
				return true;
		}

		return false;
	}
	bool IsTroughSwitchOff(InputEvent input)
	{
		for (int i = 0; i < TroughOptions.Switches.Length; i++)
		{
			if (pinGod.SwitchOff(TroughOptions.Switches[i], input))
				return true;
		}

		return false;
	}
	private void OnMultiballStarted()
	{		
		_mballSaveSecondsRemaining = TroughOptions.MballSaveSeconds;		
		StartBallSaver(TroughOptions.MballSaveSeconds);
		pinGod.LogInfo("trough: mball starting save for ", _mballSaveSecondsRemaining);
		CallDeferred("_startMballTrough", 1f);
	}
	/// <summary>
	/// Sets the shoot again lamp / or led state
	/// </summary>
	/// <param name="state"></param>
	private void UpdateLamps(LightState state)
	{
		if (!string.IsNullOrWhiteSpace(TroughOptions.BallSaveLamp))
		{
			pinGod.SetLampState(TroughOptions.BallSaveLamp, (byte)state);
		}
		else if (!string.IsNullOrWhiteSpace(TroughOptions.BallSaveLed))
		{
			pinGod.SetLedState(TroughOptions.BallSaveLed, (byte)state, ColorTranslator.ToOle(System.Drawing.Color.Yellow));
		}
	}
	#region Timers
	void _on_BallSaverTimer_timeout()
	{
		DisableBallSave();
		pinGod.EmitSignal(nameof(PinGodGameBase.BallSaveEnded));
	}
	void _trough_pulse_timeout()
	{
		_mballSaveSecondsRemaining--;
		if (_mballSaveSecondsRemaining < 1)
		{
			DisableBallSave();
			troughPulseTimer.Stop();
			pinGod.LogDebug("trough: ended mball trough pulse timer");
		}

		//put a ball into plunger_lane
		if (!pinGod.SwitchOn(TroughOptions.PlungerLaneSw) && TroughOptions.BallSaveSeconds > 0)
		{
			var ballsIntTrough = BallsInTrough();
			var b = TroughOptions.Switches.Length - ballsIntTrough;
			if (b < TroughOptions.NumBallsToSave)
			{
				pinGod.LogDebug("trough: pulse");
				PulseTrough();
			}
		}		
	}
	#endregion
}
