using Godot;
using System;

public class SwitchOverlay : GridContainer
{
	private Switches _switches;

	PinGodGame _pinGodGame;

	public override void _EnterTree()
	{
		_switches = Machine.Switches;
		foreach (var sw in _switches)
		{
			var button = new Button() { Text = sw.Key, Flat = true};
			AddChild(button);
			button.Connect("pressed", this, "OnPressed", new Godot.Collections.Array(new object[] { sw.Key}));
		}

		_pinGodGame = GetNode("/root/PinGodGame") as PinGodGame;
	}

	void OnPressed(string swName)
	{
		var sw = Machine.Switches[swName];
		sw?.SetSwitch(true);
	}
}
