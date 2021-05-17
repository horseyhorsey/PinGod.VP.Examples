using Godot;
using System;
using static Godot.GD;

public class Game : Node2D
{
	private GameGlobals gameGlobal;

	/// <summary>
	/// A bonus scene to create an instance from at the end of ball
	/// </summary>
	private PackedScene endOfBallBonus;
	/// <summary>
	/// The high score entry screen.
	/// </summary>
	private PackedScene scoreEntry;	
	private bool lastBall;

	public override void _Ready()
	{
		gameGlobal = GetNode("/root/GameGlobals") as GameGlobals;
		gameGlobal.Connect("BallEnded", this, "OnBallEnded");
		gameGlobal.Connect("BallStarted", this, "OnBallStarted");
		gameGlobal.Connect("BonusEnded", this, "OnBonusEnded");
		gameGlobal.Connect("ScoreEntryEnded", this, "OnScoreEntryEnded");		

		endOfBallBonus = Load("res://modes/common/bonus/Bonus.tscn") as PackedScene;
		scoreEntry = Load("res://modes/common/score_entry/ScoreEntry.tscn") as PackedScene;
	}

	/// <summary>
	/// Handles the trough last switch and other in-lane switches if the game isn't tilted
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (!GameGlobals.GameInPlay) return;

		//Check if the last trough switch was enabled
		if (@event.IsActionPressed("sw" + Trough.TroughSwitches[Trough.TroughSwitches.Length - 1]))
		{
			//end the ball in play?
			if (!GameGlobals.InBonusMode && GameGlobals.GameInPlay && Trough.IsTroughFull() && !Trough.BallSaveActive)
			{
				Print("game: trough switches updated");
				if (gameGlobal.EndBall())
				{					
					lastBall = true;
				}
				else
				{
					Print("game: new ball starting");
				}
			}
		}

		//game is tilted, don't process other switches when tilted
		if (GameGlobals.IsTilted) return;

		if (@event.IsActionPressed("sw21")) //Left outlane
		{
			Print("sw: l_outlane");
			AddPoints(100);        
		}
		if (@event.IsActionPressed("sw22")) //Left inlane
		{
			Print("sw: l_inlane");
			AddPoints(100);
		}
		if (@event.IsActionPressed("sw23")) //Right outlane
		{
			Print("sw: r_inlane");
			AddPoints(100);
		}
		if (@event.IsActionPressed("sw24")) //Left inlane
		{
			Print("sw: r_outlane");
			AddPoints(100);
		}
		if (@event.IsActionPressed("sw25")) //Left sling
		{
			Print("sw: l_sling");
			AddPoints(50);
		}
		if (@event.IsActionPressed("sw26"))//Right sling
		{
			Print("sw: r_sling");
			AddPoints(50);
		}
	}

	/// <summary>
	/// Adds an instance of <see cref="scoreEntry"/> to the tree
	/// </summary>
	internal void AddScoreEntry()
	{
		AddChild(scoreEntry.Instance());
	}

	private void AddPoints(int points)
	{
		gameGlobal.AddPoints(points);
		gameGlobal.AddBonus(25);
	}

	/// <summary>
	/// Add a display at end of ball
	/// </summary>
	public void OnBallEnded()
	{
		Print("game: ball ended");
		//set the bonus to true, stop trough twice
		GameGlobals.InBonusMode = true;		

		if (!GameGlobals.IsTilted)
		{
			Print("game: adding bonus scene for player: " + GameGlobals.CurrentPlayerIndex);
			AddChild(endOfBallBonus.Instance());
			return;
		}
		else if (GameGlobals.IsTilted && lastBall)
		{
			GameGlobals.InBonusMode = false;
			AddChild(scoreEntry.Instance());
			return;
		}

		Print("no bonus, game was tilted");
		GameGlobals.InBonusMode = false;
		if(!lastBall)
			gameGlobal.StartNewBall();
	}

	public void OnBonusEnded()
	{
		Print("game: bonus ended, starting new ball");
		GameGlobals.InBonusMode = false;

		if(lastBall)
		{
			Print("game: last ball played, end of game");
			AddScoreEntry();
		}
		else
		{
			gameGlobal.StartNewBall();
		}		
	}

	public void OnBallStarted()
	{
		Print("game: ball started");
	}		

	/// <summary>
	/// When score entry is finished set <see cref="GameGlobals.EndOfGame"/>
	/// </summary>
	void OnScoreEntryEnded()
	{
		gameGlobal.EndOfGame();
	}
}
