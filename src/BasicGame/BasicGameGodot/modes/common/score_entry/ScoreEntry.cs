using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Godot.GD;

/// <summary>
/// Simple score entry: Sends <see cref="GameGlobals.ScoreEntryEnded"/> TODO entries, placeholder
/// </summary>
public class ScoreEntry : CanvasLayer
{
	int selectedIndex = 0;
	string entry = "";
	int PlayerCount;
	int CurrentPlayer = 0;

	private Label selectedCharLabel;
	private Label selectedName;
	private Label playerLabel;

	public override void _Ready()
	{
		//test players
		//GameGlobals.Players.Add(new PlayerBasicGame() { Points = 2430 });
		//GameGlobals.Players.Add(new PlayerBasicGame() { Points = 2530 });

		_gameGlobals = GetNode("/root/GameGlobals");
		PlayerCount = GameGlobals.Players?.Count ?? 0;

		if (PlayerCount <= 0)
		{
			PrintErr("Need players for this mode to cycle through");
			QuitScoreEntry();
			return;
		}

		CharSelectionSetup();

		selectedCharLabel = GetNode("SelectedChar") as Label;
		selectedName = GetNode("Name") as Label;
		selectedName.Text = entry;

		playerLabel = GetNode("Label") as Label;

		MoveNextPlayer();
		
	}

	private void QuitScoreEntry()
	{
		Print("score_entry: quit score entry");				
		this.QueueFree();
		_gameGlobals.EmitSignal("ScoreEntryEnded");
	}

	private PinGodPlayer _cPlayer;
	private bool MoveNextPlayer()
	{
		if(CurrentPlayer > PlayerCount - 1)
		{
			return false;
		}

		//reset the entry player initials
		entry = string.Empty;		
		//get the player to check hi scores
		_cPlayer = GameGlobals.Players[CurrentPlayer];

		playerLabel.Text = $"PLAYER {CurrentPlayer + 1}\r\nENTER NAME";

		//hi scores has enough room to add new at any points
		if (GameGlobals.GameData.HighScores.Count < GameGlobals.GameSettings.MaxHiScoresCount)
		{
			Print("Hi score has space, adding this player");
			CurrentPlayer++;
		}
		//this hi score isn't as big as the others
		else if (!GameGlobals.GameData.HighScores.Any(x => x.Scores < _cPlayer.Points))
		{
			Print("hi score not large enough for board");
			CurrentPlayer++;
			if (!MoveNextPlayer())
			{
				QuitScoreEntry();
				return false;
			}
		}
		else
		{
			CurrentPlayer++;
		}		

		return true;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("sw" + GameGlobals.FlipperRSwitchNum))
		{
			//allowedChars
			selectedIndex--;
		}

		if (@event.IsActionPressed("sw" + GameGlobals.FlipperLSwitchNum))
		{
			selectedIndex++;
		}

		if (@event.IsActionPressed("sw" + GameGlobals.StartSwitchNum))
		{
			//delete char
			if (allowedChars[selectedIndex] == 60)
			{
				if(entry.Length > 0)
					entry = entry.Remove(entry.Length - 1);
			}				
			//accept
			else if (allowedChars[selectedIndex] == 61)
			{
				//add the hi score and order it
				GameGlobals.GameData.HighScores.Add(new HighScore()
				{
					Name = entry, Created = DateTime.Now, Scores = _cPlayer.Points
				});
				GameGlobals.GameData.HighScores = GameGlobals.GameData.HighScores.OrderByDescending(x => x.Scores)
					.Take(GameGlobals.GameSettings.MaxHiScoresCount)
					.ToList();
				Print("hi score added", entry, _cPlayer.Points);

				if (!MoveNextPlayer())
				{					
					QuitScoreEntry();
					return;
				}
			}
			else
			{
				entry += (char)allowedChars[selectedIndex];
			}			
		}

		if (selectedIndex > allowedChars.Length-1)
			selectedIndex = 0;
		else if(selectedIndex < 0)
			selectedIndex = allowedChars.Length-1;

		selectedCharLabel.Text = ((char)allowedChars[selectedIndex]).ToString();
		selectedName.Text = entry;
	}


	int[] allowedChars;
	private Node _gameGlobals;

	private void CharSelectionSetup()
	{
		var chars = new List<int>();
		chars.AddRange(Enumerable.Range(65, 26)); //A-Z
		chars.AddRange(Enumerable.Range(48, 10)); //0-9
		chars.Add(60); //delete
		chars.Add(61); //enter
		allowedChars = chars.ToArray();
	}
}
