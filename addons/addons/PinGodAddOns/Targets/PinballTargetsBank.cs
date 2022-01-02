using Godot;

public class PinballTargetsBank : Node
{
    #region Exports
    [Export] protected string[] _target_switches;
    [Export] protected string[] _target_lamps;
    [Export] protected string[] _target_leds;
    [Export] protected bool _inverse_lamps;
    [Export] protected bool _reset_when_completed = true;
    #endregion

    #region Signals
    /// <summary>
    /// Fired when all <see cref="_targetValues"/> are complete
    /// </summary>
    [Signal] protected delegate void OnTargetsCompleted();

    [Signal] protected delegate void OnTargetActivated(string swName, bool complete);
    #endregion

    #region Fields
    public bool[] _targetValues;
    protected PinGodGameBase pinGod;
    protected bool _targetsCompleted;
    #endregion

    public override void _EnterTree()
    {
        if (!Engine.EditorHint)
        {
            pinGod = GetNode("/root/PinGodGame") as PinGodGameBase;

            if (_target_switches == null)
            {
                pinGod.LogError("no target switches assigned. removing mode");
                this.QueueFree();
            }
            else
            {
                pinGod.LogDebug("setting target values");
                _targetValues = new bool[_target_switches.Length];
                pinGod.LogDebug(_targetValues.Length);
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!Engine.EditorHint)
        {
            ProcessTargetSwitchInputs(@event);
        }
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

    /// <summary>
    /// Sets the next target to complete
    /// </summary>
    internal int AdvanceTarget()
    {
        if (!_targetsCompleted)
        {
            for (int i = 0; i < _targetValues.Length; i++)
            {
                if (!_targetValues[i])
                {
                    SetTargetComplete(i);
                    return i;
                }
            }
        }

        return -1;
    }

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

    public void ResetTargets()
    {
        _targetValues = new bool[_target_switches.Length];
        _targetsCompleted = false;
    }

    /// <summary>
    /// Returns whether the target was set or not. Emits <see cref="OnTargetActivated"/>
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual bool SetTargetComplete(int index)
    {
        if (!_targetValues[index])
        {
            _targetValues[index] = true;
            UpdateLamps();
        }

        EmitSignal(nameof(OnTargetActivated), new object[] { _target_switches[index], _targetValues[index] });
        return _targetValues[index];
    }


    /// <summary>
    /// Targets were completed, reset values?
    /// </summary>
    /// <param name="reset"></param>
    public virtual void TargetsCompleted(bool reset = true)
    {
        if (!_targetsCompleted)
        {
            _targetsCompleted = true;
            EmitSignal(nameof(OnTargetsCompleted));

            if (_reset_when_completed)
            {
                pinGod.LogDebug("targets complete, resetting");
                ResetTargets();
            }
            else
            {
                pinGod.LogDebug("targets complete");
            }
        }
    }

    /// <summary>
    /// Updates the lamps with matched to the same length as the switches
    /// </summary>
    public virtual void UpdateLamps()
    {
        if (_target_lamps?.Length > 0)
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
}