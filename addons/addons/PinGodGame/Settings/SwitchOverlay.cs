using Godot;

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
			var button = new Button() { Text = sw.Key, ToggleMode = true };
			AddChild(button);
			button.Connect("toggled", this, "OnToggle", new Godot.Collections.Array(new object[] { sw.Key }));
		}

		pingod = GetNode("/root/PinGodGame") as PinGodGameBase;
	}

	void OnToggle(bool button_pressed, string swName)
	{
		//pingod.LogDebug("switch overlay: " + swName + button_pressed);
		var sw = Machine.Switches[swName];
		sw?.SetSwitch(button_pressed);
	}
}
