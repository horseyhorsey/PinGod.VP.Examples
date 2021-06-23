using Godot;

/// <summary>
/// A base node for managing a set of targets. Handling switches, lamps, when completed
/// </summary>
public abstract class PinballTargetsNode : Node
{
	#region Exports
	[Export] protected string[] _target_switches;
	[Export] protected string[] _target_lamps;
	[Export] protected bool _enable_logging = true;
	#endregion

	#region Fields
	private bool[] _targetValues;
	protected PinGodGameBase pinGod;
	#endregion

	#region Godot

	public override void _Ready()
	{
		if (_target_switches == null)
		{
			GD.PushError("no target switches assigned. removing mode");
			this.QueueFree();
		}
		else
		{
			_targetValues = new bool[_target_switches.Length];
			pinGod = GetNode("/root/PinGodGame") as PinGodGameBase;
		}
	}

	public override void _Input(InputEvent @event)
	{
		ProcessTargetSwitchInputs(@event);
	}
	#endregion

	#region Public Methods
	public virtual bool CheckTargetsCompleted(int index)
	{
		_targetValues[index] = true;

		for (int i = 0; i < _targetValues.Length; i++)
		{
			if (!_targetValues[i]) return false;
		}

		return true;
	}

	/// <summary>
	/// Processes the <see cref="_target_switches"/>. 
	/// </summary>
	/// <param name="event"></param>
	public virtual void ProcessTargetSwitchInputs(InputEvent @event)
	{
		if (_target_switches?.Length > 0)
		{
			for (int i = 0; i < _target_switches.Length; i++)
			{
				if (pinGod.SwitchOn(_target_switches[i], @event))
				{
					if (_enable_logging)
						GD.Print("target: activated: ", _target_switches[i]);

					if (CheckTargetsCompleted(i))
					{
						TargetsCompleted();
					}
				}
			}
		}
	}

	public void ResetTargets() => _targetValues = new bool[_target_switches.Length];

	/// <summary>
	/// Targets were completed, reset values?
	/// </summary>
	/// <param name="reset"></param>
	public virtual void TargetsCompleted(bool reset = true)
	{
		if (reset)
		{
			GD.Print("targets complete, resetting");
			ResetTargets();			
		}
		else
		{
			GD.Print("targets complete");
		}
	}

	public virtual void UpdateLamps()
	{
		for (int i = 0; i < _target_switches.Length; i++)
		{
			if (_targetValues[i]) pinGod.SetLampState(_target_lamps[i], 1);
			else pinGod.SetLampState(_target_lamps[i], 0);
		}
	} 
	#endregion
}
