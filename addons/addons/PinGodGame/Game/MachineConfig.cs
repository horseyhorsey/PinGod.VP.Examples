using Godot;
using Godot.Collections;
using System.Linq;

/// <summary>
/// Config
/// </summary>
public class MachineConfig : Node
{
	/// <summary>
	/// Must reflect VP controller counts
	/// </summary>
	[Export] public byte _memCoilCount = 32;
	/// <summary>
	/// Must reflect VP controller counts
	/// </summary>
	[Export] public byte _memLampCount = 64;
	/// <summary>
	/// Must reflect VP controller counts
	/// </summary>
	[Export] public byte _memLedCount = 64;
	/// <summary>
	/// Must reflect VP controller counts
	/// </summary>
	[Export] public byte _memSwitchCount = 64;

	[Export] Dictionary<string, byte> _coils = new Dictionary<string, byte>();
	[Export] Dictionary<string, byte> _switches = new Dictionary<string, byte>();
	[Export] Dictionary<string, byte> _lamps = new Dictionary<string, byte>();
	[Export] Dictionary<string, byte> _leds = new Dictionary<string, byte>();
	/// <summary>
	/// 
	/// </summary>
	[Export] public bool _ball_search_enabled = true;
	/// <summary>
	/// Coil names to pulse when ball searching
	/// </summary>
	[Export] public string[] _ball_search_coils;
	/// <summary>
	/// Switches that stop the ball searching
	/// </summary>
	[Export] public string[] _ball_search_stop_switches;
	/// <summary>
	/// How long to wait for ball searching and reset
	/// </summary>
	[Export] private int _ball_search_wait_time_secs = 10;
	/// <summary>
	/// 
	/// </summary>
    public BallSearchOptions BallSearchOptions { get; private set; }

    /// <summary>
    /// <see cref="AddCustomMachineItems"/>
    /// </summary>
    public override void _EnterTree()
    {
        if (!Engine.EditorHint)
        {
			//ball search options
			BallSearchOptions = new BallSearchOptions(_ball_search_coils, _ball_search_stop_switches, _ball_search_wait_time_secs, _ball_search_enabled);

			AddCustomMachineItems(_coils, _switches, _lamps, _leds);
		}		
	}

    /// <summary>
    /// Adds custom machine items. Actions are created for switches if they do not exist
    /// </summary>
    /// <param name="coils"></param>
    /// <param name="switches"></param>
    /// <param name="lamps"></param>
    /// <param name="leds"></param>
    protected void AddCustomMachineItems(Dictionary<string, byte> coils, Dictionary<string, byte> switches, Dictionary<string, byte> lamps, Dictionary<string, byte> leds)
	{
		foreach (var coil in coils.Keys)
		{
			Machine.Coils.Add(coil, new PinStateObject(coils[coil]));
		}
		var itemAddResult = string.Join(", ", coils.Keys);
		Logger.LogDebug($"pingod: added coils {itemAddResult}");
		coils.Clear();

		foreach (var sw in switches.Keys)
		{
			//create an action for the switch if it doesn't exist.
			var swVal = switches[sw];
			if (!Godot.InputMap.HasAction("sw" + swVal))
			{
				Godot.InputMap.AddAction("sw" + swVal);
			}

			if (BallSearchOptions.StopSearchSwitches?.Any(x => x == sw) ?? false)
			{
				Machine.Switches.Add(sw, new Switch(swVal, BallSearchSignalOption.Off));
			}
			else
			{
				Machine.Switches.Add(sw, new Switch(swVal, BallSearchSignalOption.Reset));
			}
		}

		itemAddResult = string.Join(", ", switches.Keys);
		//LogDebug($"pingod: added switches {itemAddResult}");
		switches.Clear();

		foreach (var lamp in lamps.Keys)
		{
			Machine.Lamps.Add(lamp, new PinStateObject(lamps[lamp]));
		}
		//itemAddResult = string.Join(", ", lamps.Keys);
		//LogDebug($"pingod: added lamps {itemAddResult}");
		lamps.Clear();

		foreach (var led in leds.Keys)
		{
			Machine.Leds.Add(led, new PinStateObject(leds[led]));
		}
		//LogDebug($"pingod: added leds {string.Join(", ", leds.Keys)}");
		leds.Clear();
	}
}