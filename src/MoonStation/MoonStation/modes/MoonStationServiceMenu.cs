using Godot;
using MoonStation.game;

public class MoonStationServiceMenu : ServiceMenu
{
	string[] CurrentMenuItems = null;
	string[] Items = new string[] { "music"};
	string[] MusicOptions = new string[] { "off", "dnb", "techno" };	
	int selectedIndex = 0;

	bool inMusicMenu = false;
    private MsPinGodGame msGame;

    public override void _Ready()
	{
		menuNameLabel = GetNode("CenterContainer/Label") as Label;
		CurrentMenuItems = Items;
		menuNameLabel.Text = Items[0];

		msGame = pinGod as MsPinGodGame;
	}

    public override void OnDown()
    {
		base.OnDown();

		selectedIndex--;
		if (selectedIndex < 0)
		{
			selectedIndex = CurrentMenuItems.Length - 1;
		}

		UpdateText();
	}

    public override void OnUp()
    {
		base.OnUp();

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
        else
        {

        }

		CurrentMenuItems = Items;
		selectedIndex = 0;
		inMusicMenu = false;
		UpdateText();
    }

    public override void OnEnter()
    {
		base.OnEnter();

		var menu = CurrentMenuItems[selectedIndex];
		pinGod.LogDebug("selected menu", menu);		
		if (inMusicMenu)
		{
			switch (menu)
			{
				case "off":
					msGame.SetMusicOff();
					break;
                case "dnb":
                case "techno":
					msGame.SetMusicOn(menu);                    
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
