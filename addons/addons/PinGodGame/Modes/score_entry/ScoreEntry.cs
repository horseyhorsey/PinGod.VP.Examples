using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simple score entry: Sends <see cref="PinGodGame.ScoreEntryEnded"/> TODO entries, placeholder
/// </summary>
public class ScoreEntry : Control
{
    protected PinGodGame pinGod;
    private PinGodPlayer _cPlayer;
    [Export] int _nameMaxLength = 8;
    int[] allowedChars;
    int CurrentPlayer = 0;
    string entry = "";
    bool IsPlayerEnteringScore = false;
    int PlayerCount;
    private Label playerLabel;
    private Label selectedCharLabel;
    int selectedIndex = 0;
    private Label selectedName;

    public override void _Input(InputEvent @event)
    {
        if (this.Visible && IsPlayerEnteringScore)
        {
            if (pinGod.SwitchOn("flipper_l", @event))
            {
                selectedIndex++;
            }
            if (pinGod.SwitchOn("flipper_r", @event))
            {
                selectedIndex--;
            }
            if (pinGod.SwitchOn("start", @event))
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
                    OnPlayerFinishedEntry();
                }
                else
                {
                    if (entry.Length < _nameMaxLength)
                    {
                        entry += (char)allowedChars[selectedIndex];
                    }
                    else
                    {
                        OnPlayerFinishedEntry();
                    }
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

    public override void _Ready()
    {
		//test players
		//GameGlobals.Players.Add(new PlayerBasicGame() { Points = 2430 });
		//GameGlobals.Players.Add(new PlayerBasicGame() { Points = 2530 });

		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		CharSelectionSetup();
		selectedCharLabel = GetNode("CenterContainer/VBoxContainer/SelectedChar") as Label;
		selectedName = GetNode("CenterContainer/VBoxContainer/Name") as Label;
		selectedName.Text = entry;
		playerLabel = GetNode("CenterContainer/VBoxContainer/Label") as Label;
	}

	public void DisplayHighScore()
	{
		IsPlayerEnteringScore = true;
		this.Visible = true;
		PlayerCount = pinGod.Players?.Count ?? 0;
		if (PlayerCount <= 0)
		{
			pinGod.LogError("Need players for this mode to cycle through");
			QuitScoreEntry();
			return;
		}
		MoveNextPlayer();
	}

	public virtual void OnNewHighScore() 
	{
		pinGod.LogDebug("new high score made");
	}

    private void CharSelectionSetup()
    {
        var chars = new List<int>();
        chars.AddRange(Enumerable.Range(65, 26)); //A-Z
        chars.AddRange(Enumerable.Range(48, 10)); //0-9
        chars.Add(60); //delete
        chars.Add(61); //enter
        allowedChars = chars.ToArray();
    }

    private bool MoveNextPlayer()
    {
        if (CurrentPlayer > PlayerCount - 1)
        {
            return false;
        }

        //reset the entry player initials
        entry = string.Empty;
        //get the player to check hi scores
        _cPlayer = pinGod.Players[CurrentPlayer];

        playerLabel.Text = Tr("HI_SCORE_ENTRY").Replace("%d", (CurrentPlayer + 1).ToString());

        //hi scores has enough room to add new at any points
        if (pinGod.GameData.HighScores.Count < pinGod.GameSettings.MaxHiScoresCount)
        {
            pinGod.LogDebug("Hi score has space, adding this player");
            CurrentPlayer++;
        }
        //this hi score isn't as big as the others
        else if (!pinGod.GameData.HighScores.Any(x => x.Scores < _cPlayer.Points))
        {
            pinGod.LogDebug("hi score not large enough for board");
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

        OnNewHighScore();
        return true;
    }

    void OnPlayerFinishedEntry()
    {
        pinGod.GameData.HighScores.Add(new HighScore()
        {
            Name = entry,
            Created = DateTime.Now,
            Scores = _cPlayer.Points
        });
        pinGod.GameData.HighScores = pinGod.GameData.HighScores.OrderByDescending(x => x.Scores)
            .Take(pinGod.GameSettings.MaxHiScoresCount)
            .ToList();
        pinGod.LogInfo("hi score added", entry, _cPlayer.Points);

        if (!MoveNextPlayer())
        {
            QuitScoreEntry();
            return;
        }
    }

    private void QuitScoreEntry()
    {
		IsPlayerEnteringScore = false;
		this.Visible = false;
		pinGod.LogInfo("score_entry: quit score entry");
		pinGod.EmitSignal("ScoreEntryEnded");
	}
}
