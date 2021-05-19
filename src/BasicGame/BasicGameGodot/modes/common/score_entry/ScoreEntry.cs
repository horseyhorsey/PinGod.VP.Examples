using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Godot.GD;

/// <summary>
/// Simple score entry: Sends <see cref="PinGodGame.ScoreEntryEnded"/> TODO entries, placeholder
/// </summary>
public class ScoreEntry : Control
{
	int selectedIndex = 0;
	string entry = "";
	int PlayerCount;
	int CurrentPlayer = 0;

	private Label selectedCharLabel;
	private Label selectedName;
	private Label playerLabel;

	private PinGodGame pinGodGame;
	private PinGodPlayer _cPlayer;

	bool IsPlayerEnteringScore = false;

	public override void _Ready()
	{
		//test players
		//GameGlobals.Players.Add(new PlayerBasicGame() { Points = 2430 });
		//GameGlobals.Players.Add(new PlayerBasicGame() { Points = 2530 });

		pinGodGame = GetNode("/root/PinGodGame") as PinGodGame;
		CharSelectionSetup();
		selectedCharLabel = GetNode("SelectedChar") as Label;
		selectedName = GetNode("Name") as Label;
		selectedName.Text = entry;
		playerLabel = GetNode("Label") as Label;		
	}

	public void DisplayHighScore()
	{
		IsPlayerEnteringScore = true;
		this.Visible = true;
		PlayerCount = PinGodGameBase.Players?.Count ?? 0;
		if (PlayerCount <= 0)
		{
			PrintErr("Need players for this mode to cycle through");
			QuitScoreEntry();
			return;
		}
		MoveNextPlayer();
	}

	private void QuitScoreEntry()
	{
		IsPlayerEnteringScore = false;
		this.Visible = false;
		Print("score_entry: quit score entry");
		pinGodGame.EmitSignal("ScoreEntryEnded");
	}
	
	private bool MoveNextPlayer()
	{
		if(CurrentPlayer > PlayerCount - 1)
		{
			return false;
		}

		//reset the entry player initials
		entry = string.Empty;		
		//get the player to check hi scores
		_cPlayer = PinGodGame.Players[CurrentPlayer];

		playerLabel.Text = $"PLAYER {CurrentPlayer + 1}\r\nENTER NAME";

		//hi scores has enough room to add new at any points
		if (PinGodGame.GameData.HighScores.Count < PinGodGame.GameSettings.MaxHiScoresCount)
		{
			Print("Hi score has space, adding this player");
			CurrentPlayer++;
		}
		//this hi score isn't as big as the others
		else if (!PinGodGame.GameData.HighScores.Any(x => x.Scores < _cPlayer.Points))
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
		if (this.Visible && IsPlayerEnteringScore)
		{
			if (pinGodGame.SwitchOn("flipper_l", @event))
			{
				selectedIndex++;
			}
			if (pinGodGame.SwitchOn("flipper_r", @event))
			{
				selectedIndex--;
			}
			if (pinGodGame.SwitchOn("start", @event))
			{
				//delete char
				if (allowedChars[selectedIndex] == 60)
				{
					if (entry.Length > 0)
						entry = entry.Remove(entry.Length - 1);
				}
				//accept
				else if (allowedChars[selectedIndex] == 61)
				{
					//add the hi score and order it
					PinGodGame.GameData.HighScores.Add(new HighScore()
					{
						Name = entry,
						Created = DateTime.Now,
						Scores = _cPlayer.Points
					});
					PinGodGame.GameData.HighScores = PinGodGame.GameData.HighScores.OrderByDescending(x => x.Scores)
						.Take(PinGodGame.GameSettings.MaxHiScoresCount)
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

			if (selectedIndex > allowedChars.Length - 1)
				selectedIndex = 0;
			else if (selectedIndex < 0)
				selectedIndex = allowedChars.Length - 1;

			selectedCharLabel.Text = ((char)allowedChars[selectedIndex]).ToString();
			selectedName.Text = entry;
		}		
	}

	int[] allowedChars;	
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
