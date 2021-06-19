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

	private PinGodGameBase pinGod;

	/// <summary>
	/// Ball saver timer. Setup in <see cref="_Ready"/>
	/// </summary>
	private Timer ballSaverTimer;
	private Timer troughPulseTimer;

	public TroughOptions TroughOptions { get; set; }

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGameBase;
		pinGod.Connect(nameof(PinGodGameBase.MultiballStarted), this, "OnMultiballStarted");	
		ballSaverTimer = GetNode("BallSaverTimer") as Timer;
		troughPulseTimer = GetNode("TroughPulseTimer") as Timer;
		Print("trough:_enter tree");
	}

	public override void _Ready()
	{
		Print("trough:_ready. switch_count: ", TroughOptions?.Switches.Length);
	}

	/// <summary>
	/// Listen for actions. Mainly from trough switches and plunger lanes.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{

		//fire early saves
		if (!pinGod.IsBallStarted && !pinGod.IsTilted && pinGod.BallSaveActive)
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
		if (IsTroughSwitch(@event))
		{
			var troughFull = IsTroughFull();
			var balls = BallsInTrough();

			if (troughFull && pinGod.IsBallStarted && !pinGod.IsMultiballRunning)
			{				
				if (pinGod.BallSaveActive && !pinGod.IsTilted)
				{
					Print("trough: ball_saved");
					pinGod.SolenoidPulse(TroughOptions.AutoPlungerCoil);
					pinGod.EmitSignal(nameof(PinGodGameBase.BallSaved));
				}
				else
				{
					pinGod.IsBallStarted = false;
					pinGod.EmitSignal(nameof(PinGodGameBase.BallDrained));
				}
			}
			else if(pinGod.IsMultiballRunning && !pinGod.BallSaveActive)
			{
				if (TroughOptions.Switches.Length - 1 == balls)
				{
					pinGod.IsMultiballRunning = false;
					troughPulseTimer.Stop();
					pinGod.EmitSignal(nameof(PinGodGameBase.MultiBallEnded));
				}
			}
		}

		if (!pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return;

		//when the ball is in the plunger lane stop any ball search running TODO
		if (pinGod.SwitchOn(TroughOptions.PlungerLaneSw, @event))
		{
			//auto plunge the ball if in ball save or game is tilted to get the balls back
			if (pinGod.BallSaveActive || pinGod.IsTilted)
			{
				pinGod.SolenoidPulse(TroughOptions.AutoPlungerCoil, 125);
				Print("trough: auto saved");
			}
		}

		//reset the ball search when leaving the switch
		if (pinGod.SwitchOff(TroughOptions.PlungerLaneSw, @event))
		{
			//start a ball saver if game in play
			if (pinGod.GameInPlay && !pinGod.IsTilted && !pinGod.IsMultiballRunning)
			{
				var saveStarted = StartBallSaver();
				if (saveStarted)
				{
					UpdateLamps(LightState.Blink);
					pinGod.EmitSignal(nameof(PinGodGameBase.BallSaveStarted));
				}

				Print($"trough: ball_save_started? {saveStarted}");
			}
		}
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

	public int BallsInPlay() => TroughOptions.Switches.Length - BallsInTrough();

	public async void OnMultiballStarted()
	{
		Print("trough: multiball starting");
		await Task.Run(() =>
		{
			pinGod.BallSaveActive = true;

			UpdateLamps(LightState.Blink);

			ballSaverTimer.Start(TroughOptions.MballSaveSeconds);
			Print("trough: mball started, save time:", TroughOptions.MballSaveSeconds);

			//let balls go all time in a multiball
			CallDeferred("_startMballTrough", 2f);
		});
	}

	/// <summary>
	/// Activates the ball saver if not already running. Blinks the ball saver lamp
	/// </summary>
	/// <returns>True if the ball saver is active</returns>
	public bool StartBallSaver()
	{
		if (!pinGod.BallSaveActive)
		{
			pinGod.BallSaveActive = true;
			UpdateLamps(LightState.Blink);
			ballSaverTimer.Start(TroughOptions.BallSaveSeconds);
		}

		return pinGod.BallSaveActive;
	}

	public void DisableBallSave()
	{
		pinGod.BallSaveActive = false;
		UpdateLamps(LightState.Off);
	}

	private void FireEarlySave()
	{
		Print("trough:early ball_saved");
		PulseTrough();
		pinGod.EmitSignal(nameof(PinGodGameBase.BallSaved));
	}

	bool IsTroughSwitch(InputEvent input)
	{
		for (int i = 0; i < TroughOptions.Switches.Length; i++)
		{
			if (pinGod.SwitchOn(TroughOptions.Switches[i], input))
				return true;
		}

		return false;
	}

	public void PulseTrough()
	{
		pinGod.SolenoidPulse(TroughOptions.Coil);
	}

	/// <summary>
	/// Starts multi-ball trough
	/// </summary>
	/// <param name="numOfBalls"></param>
	/// <param name="ballSaveTime"></param>
	/// <param name="pulseTimerDelay">Timer to pulse trough</param>
	internal void StartMultiball(byte numOfBalls, byte ballSaveTime, float pulseTimerDelay = 1)
	{
		TroughOptions.MballSaveSeconds = ballSaveTime;
		TroughOptions.NumBallsToSave = numOfBalls;
		if (pulseTimerDelay > 0)
			_startMballTrough(pulseTimerDelay);
	}

	void _startMballTrough(float delay) => troughPulseTimer.Start(delay);

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
		TroughOptions.MballSaveSeconds--;
		if (TroughOptions.MballSaveSeconds < 1)
		{
			DisableBallSave();
			troughPulseTimer.Stop();
			Print("trough: ended mball trough pulse timer");
		}

		//put a ball into plunger_lane
		if (!pinGod.SwitchOn(TroughOptions.PlungerLaneSw) && TroughOptions.BallSaveSeconds > 0)
		{
			var ballsIntTrough = BallsInTrough();
			var b = TroughOptions.Switches.Length - ballsIntTrough;
			if (b < TroughOptions.NumBallsToSave)
			{
				Print("trough: pulse");
				PulseTrough();
			}
		}		
	}
	#endregion
}
