using Godot;

public class MoonStationServiceMenu : ServiceMenu
{
	string[] CurrentMenuItems = null;
	string[] Items = new string[] { "music", "exit" };
	string[] MusicOptions = new string[] { "off", "dnb", "techno" };	
	int selectedIndex = 0;

	bool inMusicMenu = false;

	public override void _Ready()
	{
		menuNameLabel = GetNode("CenterContainer/Label") as Label;
		CurrentMenuItems = Items;
		menuNameLabel.Text = Items[0];
	}

    public override void OnDown()
    {
		selectedIndex--;
		if (selectedIndex < 0)
		{
			selectedIndex = CurrentMenuItems.Length - 1;
		}

		UpdateText();
	}

    public override void OnUp()
    {
		selectedIndex++;
		if (selectedIndex > CurrentMenuItems.Length - 1)
		{
			selectedIndex = 0;
		}

		UpdateText();
	}

    public override void OnExit()
    {
		if (!inMusicMenu)
		{
			base.OnExit();
			return;
		}
		CurrentMenuItems = Items;
		inMusicMenu = false;		
    }

    public override void OnEnter()
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

	void UpdateText() => menuNameLabel.Text = CurrentMenuItems[selectedIndex];
}
