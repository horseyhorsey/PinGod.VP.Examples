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
	[Export] string _trough_entry_switch_name = "trough_4";

	[Export] string[] _trough_switches = { "trough_1", "trough_2", "trough_3", "trough_4" };

	[Export] string[] _early_save_switches = { "outlane_l", "outlane_r" };

	[Export] string _trough_solenoid = "trough";

	[Export] string _auto_plunge_solenoid = "auto_plunge";

	[Export] string _plunger_lane_switch = "plunger_lane";

	[Export] string _ball_save_lamp = "shoot_again";

	[Export] string _ball_save_led = "shoot_again";

	[Export] byte _ball_save_seconds = 8;

	[Export] byte _ball_save_multiball_seconds = 8;

	[Export] byte _number_of_balls_to_save = 1;

	/// <summary>
	/// Use this to turn off trough checking, outside VP
	/// </summary>
	public static bool _isDebugTrough = false;

	#region Public Static
	/// <summary>
	/// Is ball save active
	/// </summary>
	public static bool BallSaveActive { get; internal set; }

	#endregion

	private PinGodGame pinGod;

	/// <summary>
	/// Ball saver timer. Setup in <see cref="_Ready"/>
	/// </summary>
	private Timer ballSaverTimer;
	private Timer troughPulseTimer;
	private bool _isDraining;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		pinGod.Connect(nameof(PinGodGameBase.MultiballStarted), this, "OnMultiballStarted");	
		ballSaverTimer = GetNode("BallSaverTimer") as Timer;
		troughPulseTimer = GetNode("TroughPulseTimer") as Timer;

		Print("trough:_enter tree");
	}

	public override void _Ready()
	{
		Print("trough:_ready. switch_count: ", _trough_switches.Length);
	}

	/// <summary>
	/// Listen for actions. Mainly from trough switches and plunger lanes.
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{

		//fire early saves
		if (!pinGod.IsBallStarted && !pinGod.IsTilted && BallSaveActive)
		{
			for (int i = 0; i < _early_save_switches.Length; i++)
			{
				if (pinGod.SwitchOn(_early_save_switches[i], @event))
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
			Print("trough active:", _trough_entry_switch_name, " balls:", balls);

			if (troughFull && pinGod.IsBallStarted && !pinGod.IsMultiballRunning)
			{				
				if (BallSaveActive && !pinGod.IsTilted)
				{
					Print("trough: ball_saved");
					pinGod.SolenoidPulse(_trough_solenoid);
					pinGod.EmitSignal(nameof(PinGodGameBase.BallSaved));
				}
				else
				{
					pinGod.IsBallStarted = false;
					pinGod.EmitSignal(nameof(PinGodGameBase.BallDrained));
				}
			}
			else if(pinGod.IsMultiballRunning)
			{
				if (_trough_switches.Length - 1 == balls)
				{
					pinGod.EmitSignal(nameof(PinGodGameBase.MultiBallEnded));
				}
			}
		}

		if (!pinGod.GameInPlay) return;
		if (pinGod.IsTilted) return;

		//when the ball is in the plunger lane stop any ball search running TODO
		if (pinGod.SwitchOn(_plunger_lane_switch, @event))
		{
			//auto plunge the ball if in ball save or game is tilted to get the balls back
			if (BallSaveActive || pinGod.IsTilted)
			{
				pinGod.SolenoidPulse(_auto_plunge_solenoid, 125);
				Print("trough: auto saved");
			}
		}

		//reset the ball search when leaving the switch
		if (pinGod.SwitchOff(_plunger_lane_switch, @event))
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
		for (int i = 0; i < _trough_switches.Length; i++)
		{
			if (!pinGod.SwitchOn(_trough_switches[i]))
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
		for (int i = 0; i < _trough_switches.Length; i++)
		{
			if (pinGod.SwitchOn(_trough_switches[i]))
			{
				cnt++;
			}
		}
		return cnt;
	}

	public async void OnMultiballStarted()
	{
		await Task.Run(() =>
		{
			BallSaveActive = true;

			UpdateLamps(LightState.Blink);

			ballSaverTimer.Start(_ball_save_multiball_seconds);
			Print("trough: mball started, save time:", _ball_save_multiball_seconds);

			//let balls go all time in a multiball
			CallDeferred("_startMballTrough");
		});
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
			UpdateLamps(LightState.Blink);
			ballSaverTimer.Start(_ball_save_seconds);
		}

		return BallSaveActive;
	}

	public void DisableBallSave()
	{
		BallSaveActive = false;
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
		for (int i = 0; i < _trough_switches.Length; i++)
		{
			if (pinGod.SwitchOn(_trough_switches[i], input))
				return true;
		}

		return false;
	}

	private void PulseTrough()
	{
		pinGod.SolenoidPulse(_trough_solenoid);
	}

	internal void StartMultiball(byte numOfBalls, byte ballSaveTime)
	{
		_ball_save_multiball_seconds = ballSaveTime;
		_number_of_balls_to_save = numOfBalls;
	}

	void _startMballTrough() => troughPulseTimer.Start(2);


	/// <summary>
	/// Sets the shoot again lamp / or led state
	/// </summary>
	/// <param name="state"></param>
	private void UpdateLamps(LightState state)
	{
		if (!string.IsNullOrWhiteSpace(_ball_save_lamp))
		{
			pinGod.SetLampState(_ball_save_led, (byte)state);
		}
		else if (!string.IsNullOrWhiteSpace(_ball_save_led))
		{
			pinGod.SetLedState(_ball_save_led, (byte)state, ColorTranslator.ToOle(System.Drawing.Color.Yellow));
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
		if (!pinGod.SwitchOn(_plunger_lane_switch))
		{
			if (BallsInTrough() < _number_of_balls_to_save)
			{
				PulseTrough();
			}
		}

		_ball_save_multiball_seconds--;
		Print("mball sec remain", _ball_save_multiball_seconds);
		if (_ball_save_multiball_seconds < 1)
		{
			troughPulseTimer.Stop();
			Print("trough: ended mball trough pulse timer");
			DisableBallSave();
		}
	}
	#endregion
}
