using Godot;

public class MoonStationServiceMenu : Control
{
	string[] CurrentMenuItems = null;
	string[] Items = new string[] { "music", "exit" };
	string[] MusicOptions = new string[] { "off", "dnb", "techno" };
	private Label menuNameLabel;
	int selectedIndex = 0;

	bool inMusicMenu = false;
	private PinGodGame pinGod;

	public override void _Ready()
	{
		menuNameLabel = GetNode("Label") as Label;

		CurrentMenuItems = Items;
		menuNameLabel.Text = Items[0];
	}

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
	}

	public override void _Input(InputEvent @event)
	{		
		if (pinGod.SwitchOn("enter", @event))
		{
			var menu = CurrentMenuItems[selectedIndex];
			pinGod.LogDebug("selected menu", menu);

			if (inMusicMenu)
			{
				switch (menu)
				{
					case "off":
						pinGod.AudioManager.MusicEnabled = false;
						break;
					case "dnb":
					case "techno":
						pinGod.AudioManager.MusicEnabled = true;
						pinGod.AudioManager.Bgm = menu;
						pinGod.LogDebug("selected music", pinGod.AudioManager.Bgm);
						break;
					default:
						break;
				}
			}
			else if (menu == "music")
			{
				inMusicMenu = true;
				selectedIndex = 0;
				CurrentMenuItems = MusicOptions;
				UpdateText();
			}
		}
		if (pinGod.SwitchOn("up", @event))
		{
			selectedIndex++;
			if (selectedIndex > CurrentMenuItems.Length - 1)
			{
				selectedIndex = 0;
			}

			UpdateText();
		}
		if (pinGod.SwitchOn("down", @event))
		{
			selectedIndex--;
			if (selectedIndex < 0)
			{
				selectedIndex = CurrentMenuItems.Length - 1;
			}

			UpdateText();
		}
		if (pinGod.SwitchOn("exit", @event))
		{
			if (!inMusicMenu)
			{
				pinGod.EmitSignal("ServiceMenuExit");
				this.QueueFree();				
			}
			CurrentMenuItems = Items;
			inMusicMenu = false;
		}
	}

	void UpdateText() => menuNameLabel.Text = CurrentMenuItems[selectedIndex];
}
