using Godot;

/// <summary>
/// A base node for managing a set of targets. Handling switches, lamps, when completed <para/>
/// Create a class using this and a new scene to add switches and lamps in the Godot UI <para/>
/// override <see cref="CheckTargetsCompleted(int)"/> , <see cref="TargetsCompleted"/> 
/// </summary>
public abstract class PinballTargetsControl : Control
{
	#region Exports
	[Export] protected string[] _target_switches;
	[Export] protected string[] _target_lamps;
	[Export] protected string[] _target_leds;
	[Export] protected bool _inverse_lamps;
	#endregion

	#region Fields
	protected bool[] _targetValues;
	protected PinGodGameBase pinGod;
    #endregion

    #region Godot

    public override void _EnterTree()
    {
		pinGod = GetNode("/root/PinGodGame") as PinGodGameBase;

		if (_target_switches == null)
		{
			GD.PushError("no target switches assigned. removing mode");
			this.QueueFree();
		}
		else
		{
			pinGod.LogDebug("setting target values");
			_targetValues = new bool[_target_switches.Length];
			pinGod.LogDebug(_targetValues.Length);
		}
	}

	public override void _Input(InputEvent @event)
	{
		ProcessTargetSwitchInputs(@event);
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Checks if all targets are completed
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public virtual bool CheckTargetsCompleted(int index)
	{
		for (int i = 0; i < _targetValues.Length; i++)
		{
			if (!_targetValues[i]) return false;
		}

		return true;
	}

	/// <summary>
	/// Returns whether the target was set or not
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public virtual bool SetTargetComplete(int index)
    {
		if(!_targetValues[index])
        {
			_targetValues[index] = true;
			return true;
        }

		return false;
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
					pinGod.LogDebug("target: activated: ", _target_switches[i]);

					SetTargetComplete(i);

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
			pinGod.LogDebug("targets complete, resetting");
			ResetTargets();			
		}
		else
		{
			pinGod.LogDebug("targets complete");
		}
	}

	/// <summary>
	/// Updates the lamps with matched to the same length as the switches
	/// </summary>
	public virtual void UpdateLamps()
	{
		if(_target_lamps?.Length > 0)
        {
			for (int i = 0; i < _target_switches?.Length; i++)
			{
				if (_targetValues[i]) pinGod.SetLampState(_target_lamps[i], _inverse_lamps ? (byte)0 : (byte)1);
				else pinGod.SetLampState(_target_lamps[i], _inverse_lamps ? (byte)1 : (byte)0);
			}
		}
		if (_target_leds?.Length > 0)
		{
			for (int i = 0; i < _target_leds?.Length; i++)
			{
				if (_targetValues[i]) pinGod.SetLedState(_target_leds[i], _inverse_lamps ? (byte)0 : (byte)1, 0);
				else pinGod.SetLedState(_target_leds[i], _inverse_lamps ? (byte)1 : (byte)0, 0);
			}
		}
	} 
	#endregion
}
