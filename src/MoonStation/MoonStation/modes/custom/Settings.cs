using Godot;
using System;
using static Godot.GD;

public class Settings : CanvasLayer
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	string[] CurrentMenuItems = null;
	string[] Items = new string[] { "music", "exit" };
	string[] MusicOptions = new string[] { "off", "dnb", "techno" };
	private Label menuNameLabel;
	int selectedIndex = 0;

	bool inMusicMenu = false;
	private PinGodGame pinGod;

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
	}

	public override void _Ready()
	{
		menuNameLabel = GetNode("MenuLabel") as Label;

		CurrentMenuItems = Items;
		menuNameLabel.Text = Items[0];
	}

	/// <summary>
	/// Coin door controls
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		//return
		if (@event.IsActionPressed("sw4"))
		{
			if (!inMusicMenu)
			{
				this.QueueFree();
				return;
			}
			CurrentMenuItems = Items;
			inMusicMenu = false;
		}
		//next
		if (@event.IsActionPressed("sw6"))
		{
			selectedIndex++;
			if (selectedIndex > CurrentMenuItems.Length - 1)
			{
				selectedIndex = 0;
			}

			UpdateText();
		}
		//prev
		if (@event.IsActionPressed("sw5"))
		{
			selectedIndex--;
			if (selectedIndex < 0)
			{
				selectedIndex = CurrentMenuItems.Length - 1;
			}

			UpdateText();
		}
		//enter
		if (@event.IsActionPressed("sw7"))
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
	}

	void UpdateText() => menuNameLabel.Text = CurrentMenuItems[selectedIndex];

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
}
