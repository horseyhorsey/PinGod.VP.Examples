using Godot;
using System;

/// <summary>
/// SwitchOverlayGridContainer. Creates buttons from Machine.Switches and connects to button events to fire switches
/// </summary>
public class SwitchOverlay : GridContainer
{
	private Switches _switches;

	PinGodGameBase pingod;

	public override void _EnterTree()
	{
		_switches = Machine.Switches;
		foreach (var sw in _switches)
		{
			var button = new Button() { Text = sw.Key, Flat = true};
			AddChild(button);
			button.Connect("pressed", this, "OnPressed", new Godot.Collections.Array(new object[] { sw.Key}));
			//TODO: button Released?
		}

		pingod = GetNode("/root/PinGodGame") as PinGodGameBase;
	}

	void OnPressed(string swName)
	{
		var sw = Machine.Switches[swName];
		sw?.SetSwitch(true);
	}
}
