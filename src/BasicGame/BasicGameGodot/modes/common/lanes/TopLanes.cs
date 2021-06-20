using Godot;

public class TopLanes : Control
{
	[Signal] delegate void LanesCompleted();

	#region Exports
	/// <summary>
	/// Switches to handle lane events
	/// </summary>
	[Export] string[] _lane_switches = new string[0];
	/// <summary>
	/// Lamps to update
	/// </summary>
	[Export] string[] _lane_lamps = new string[0];
	[Export] bool _flipper_changes_lanes = true;
	[Export] bool _reset_on_ball_started = true;
	[Export] bool _logging_enabled = false;
	#endregion

	bool[] _lanesCompleted;
	private PinGodGame pinGod;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		ResetLanesCompleted();

		if (_logging_enabled)
			GD.Print("top_lanes: enter tree");
	}

	/// <summary>
	/// Gets switch handler events for the given switches. Flippers change lanes
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGod.IsTilted) return;

		if(_lane_switches != null)
		{
			if (_flipper_changes_lanes)
			{
				if (pinGod.SwitchOn("flipper_l", @event))
				{
					RotateLanesRight();
					UpdateLamps();
				}
				if (pinGod.SwitchOn("flipper_r", @event))
				{
					RotateLanesLeft();
					UpdateLamps();
				}
			}

			if (_lane_switches != null)
			{
				bool wasSet = false;
				for (int i = 0; i < _lane_switches.Length; i++)
				{
					if (pinGod.SwitchOn(_lane_switches[i], @event))
					{
						//Do stuff with the switch
						LaneSwitchActivated();

						//Lane switch activated
						if (!_lanesCompleted[i])
						{
							_lanesCompleted[i] = true;
							wasSet = true;
							break;
						}
					}
				}

				if (wasSet)
				{
					var complete = CheckLanes();
					if (complete)
					{
						if (_logging_enabled)
							GD.Print("top_lanes: lanes complete");
						_lanesCompleted = new bool[_lane_switches.Length];

						EmitSignal(nameof(LanesCompleted));
					}
					else
					{
						if (_logging_enabled)
							GD.Print("top_lanes: lanes checked");
					}

					UpdateLamps();
				}
			}
		}
	}

	private void LaneSwitchActivated()
	{
		//TODO: Do stuff with the switches when activated
	}

	private bool CheckLanes()
	{
		bool complete = true;
		for (int i = 0; i < _lanesCompleted.Length; i++)
		{
			if (!_lanesCompleted[i])
			{
				complete = false;
				break;
			}
		}
		return complete;
	}

	void OnBallStarted()
	{
		if (_reset_on_ball_started)
		{
			ResetLanesCompleted();
		}		
	}

	/// <summary>
	/// Resets <see cref="_lanesCompleted"/>
	/// </summary>
	void ResetLanesCompleted()
	{
		if (_lane_switches?.Length > 0)
		{
			_lanesCompleted = new bool[_lane_switches.Length];
		}
	}

	private void RotateLanesLeft()
	{
		var firstNum = _lanesCompleted[0];
		for (int i = 0; i < _lanesCompleted.Length - 1; i++)
		{
			_lanesCompleted[i] = _lanesCompleted[i + 1];
		}
		_lanesCompleted[_lanesCompleted.Length - 1] = firstNum;

		if (_logging_enabled)
			GD.Print("top_lanes: rot left: ", string.Join(",", _lanesCompleted));
	}

	private void RotateLanesRight()
	{
		var lastNum = _lanesCompleted[_lanesCompleted.Length - 1];
		for (int i = _lanesCompleted.Length - 1; i > 0; i--)
		{
			_lanesCompleted[i] = _lanesCompleted[i - 1];
		}
		_lanesCompleted[0] = lastNum;

		if (_logging_enabled)
			GD.Print("top_lanes: rot right: ", string.Join(",", _lanesCompleted));
	}

	/// <summary>
	/// Updates <see cref="_lane_lamps"/> set from <see cref="_lanesCompleted"/>
	/// </summary>
	void UpdateLamps()
	{
		if(_lanesCompleted != null)
		{
			for (int i = 0; i < _lanesCompleted.Length; i++)
			{
				if (_lanesCompleted[i])
				{
					pinGod.SetLampState(_lane_lamps[i], 1);
				}
				else
				{
					pinGod.SetLampState(_lane_lamps[i], 0);
				}
			}
		}		
	}
}
