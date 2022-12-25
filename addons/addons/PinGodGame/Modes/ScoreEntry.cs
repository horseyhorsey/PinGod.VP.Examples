using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simple score entry: Sends <see cref="PinGodGame.ScoreEntryEnded"/> TODO entries, placeholder
/// </summary>
public class ScoreEntry : Control
{
    /// <summary>
    /// 
    /// </summary>
    protected PinGodGame pinGod;

    private PinGodPlayer _cPlayer;

    private char[] _entry;

    /// <summary>
    /// include chars 0-9
    /// </summary>
    [Export] bool _includeZeroToNine = false;

    /// <summary>
    /// player max chars
    /// </summary>
    [Export] int _nameMaxLength = 8;

    /// <summary>
    /// title message node selectable
    /// </summary>
    [Export] NodePath _playerMessage = null;

    /// <summary>
    /// space when changing between chars with flippers
    /// </summary>
    [Export] int _selectCharMargin = 25;

    /// <summary>
    /// char select node path
    /// </summary>
    [Export] NodePath _selectedChar = null;

    int[] allowedChars;
    int currentEntryIndex = 0;
    int CurrentPlayer = 0;
    string entry = "";
    bool IsPlayerEnteringScore = false;
    int PlayerCount;
    private Label playerMessageLabel;
    private Label selectedCharLabel;
    private Vector2 selectedCharLabelStartPos;
    int selectedIndex = 0;
    private Label selectedName;
    /// <summary>
    /// get ref to the labels needed for the scene
    /// </summary>
    public override void _EnterTree()
    {
        selectedCharLabel = _selectedChar == null ? null : GetNode<Label>(_selectedChar);
        playerMessageLabel = _playerMessage == null ? null : GetNode<Label>(_playerMessage);
       
        _entry = new char[_nameMaxLength];
    }

    /// <summary>
    /// Uses flippers and start button actions if visible and <see cref="IsPlayerEnteringScore"/>
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (this.Visible && IsPlayerEnteringScore)
        {
            if (pinGod.SwitchOn("flipper_l", @event))
            {
                //scroll back, move the label to the left
                selectedIndex--;
                if (selectedIndex < 0)
                {
                    selectedIndex = allowedChars.Length - 1;
                    
                    selectedCharLabel.SetPosition(new Vector2(selectedCharLabel.RectPosition.x - (_selectCharMargin * allowedChars.Length - _selectCharMargin), selectedCharLabel.RectPosition.y));
                }
                else
                {
                    selectedCharLabel.SetPosition(new Vector2(selectedCharLabel.RectPosition.x + _selectCharMargin, selectedCharLabel.RectPosition.y));
                }

                _entry[currentEntryIndex] = (char)allowedChars[selectedIndex];
                selectedName.Text = new string(_entry);
            }
            if (pinGod.SwitchOn("flipper_r", @event))
            {
                //scroll forward, move the label to the right
                selectedIndex++;
                if (selectedIndex > allowedChars.Length - 1)
                {
                    selectedIndex = 0;
                    selectedCharLabel.SetPosition(new Vector2(selectedCharLabel.RectPosition.x + (_selectCharMargin * allowedChars.Length - _selectCharMargin), selectedCharLabel.RectPosition.y));
                    pinGod.LogInfo("set flip r start");
                }
                else if (selectedIndex == 0)
                {                    
                    selectedCharLabel.SetPosition(selectedCharLabelStartPos);
                }
                else { selectedCharLabel.SetPosition(new Vector2(selectedCharLabel.RectPosition.x - _selectCharMargin, selectedCharLabel.RectPosition.y)); }

                _entry[currentEntryIndex] = (char)allowedChars[selectedIndex];
                selectedName.Text = new string(_entry);
            }
            if (pinGod.SwitchOn("start", @event))
            {
                //delete char
                if (allowedChars[selectedIndex] == 60)
                {
                    if (_entry.Length > 0)
                    {
                        _entry[_entry.Length - 1] = ' ';
                        currentEntryIndex--;
                        if (currentEntryIndex < 0)
                            currentEntryIndex = 0;
                    }                        
                }
                //accept
                else if (allowedChars[selectedIndex] == 61)
                {
                    //add the hi score and order it
                    OnPlayerFinishedEntry();
                }
                else
                {
                    if (currentEntryIndex < _nameMaxLength-1)
                    {
                        currentEntryIndex++;
                        selectedName.Text = new string(_entry);
                    }
                    else
                    {
                        //OnPlayerFinishedEntry();
                        //todo: move to last char
                        selectedIndex = allowedChars.Length - 1;

                        selectedCharLabel.SetPosition(new Vector2(selectedCharLabelStartPos.x + (_selectCharMargin * allowedChars.Length-1), selectedCharLabelStartPos.y));
                        selectedName.Text = new string(_entry);
                    }
                }               
            }            
        }
    }

    /// <summary>
    /// Sets <see cref="IsPlayerEnteringScore"/> to true
    /// </summary>
    public override void _Ready()
    {
		//test players
		//GameGlobals.Players.Add(new PlayerBasicGame() { Points = 2430 });
		//GameGlobals.Players.Add(new PlayerBasicGame() { Points = 2530 });

		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		CharSelectionSetup();
		selectedName = GetNode("CenterContainer/VBoxContainer/Name") as Label;		

        IsPlayerEnteringScore = true;
        selectedCharLabelStartPos = new Vector2(selectedCharLabel.RectPosition.x, selectedCharLabel.RectPosition.y);
    }

    /// <summary>
    /// Sets <see cref="IsPlayerEnteringScore"/> to true and shows this scene. Moves to each player who has a high score to let them enter their initials
    /// </summary>
	public virtual void DisplayHighScore()
	{
        pinGod.LogInfo("display high score");
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

    /// <summary>
    /// just logs by default, override this method to act on new high scores.
    /// </summary>
	public virtual void OnNewHighScore() 
	{
		pinGod.LogDebug("new high score made");
	}

    /// <summary>
    /// Checks if player made top score
    /// </summary>
    /// <returns></returns>
    protected virtual bool MoveNextPlayer()
    {
        if (CurrentPlayer > PlayerCount - 1)
        {
            return false;
        }

        //reset the entry player initials
        entry = string.Empty;
        //get the player to check hi scores
        _cPlayer = pinGod.Players[CurrentPlayer] ?? new PinGodPlayer() { Points = 1000000 };

        if (playerMessageLabel != null)
            playerMessageLabel.Text = Tr("HI_SCORE_ENTRY").Replace("%d", (CurrentPlayer + 1).ToString());

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

    private void CharSelectionSetup()
    {
        var chars = new List<int>();
        chars.AddRange(Enumerable.Range(65, 26)); //A-Z

        if(_includeZeroToNine)
            chars.AddRange(Enumerable.Range(48, 10)); //0-9

        chars.Add(60); //delete
        chars.Add(61); //enter
        allowedChars = chars.ToArray();
    }
    /// <summary>
    /// 
    /// </summary>
    public virtual void OnPlayerFinishedEntry()
    {
        if(pinGod.GameData?.HighScores != null)
        {
            try
            {
                pinGod.GameData?.HighScores?.Add(new HighScore()
                {
                    Name = new string(_entry),
                    Created = DateTime.Now,
                    Scores = _cPlayer.Points
                });
                pinGod.GameData.HighScores = pinGod.GameData.HighScores.OrderByDescending(x => x.Scores)
                    .Take(pinGod.GameSettings.MaxHiScoresCount)
                    .ToList();
                pinGod.LogInfo("hi score added", entry, _cPlayer.Points);
            }
            catch (Exception ex) { pinGod.LogWarning("player finish score adding to hi scores. " + ex.ToString()); }
        }        

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
