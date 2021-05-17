using Godot;
using System;
using System.Collections.Generic;
using static Godot.GD;

/// <summary>
/// Godot singleton to hold Common pinball variables
/// </summary>
public class GameGlobals : PinGodGameBase
{
	#region Signals
	[Signal] public delegate void BallEnded();
	[Signal] public delegate void BallStarted();
	[Signal] public delegate void BonusEnded();
	[Signal] public delegate void CreditAdded();
	[Signal] public delegate void GameEnded();
	[Signal] public delegate void GamePaused();
	[Signal] public delegate void GameResumed();
	[Signal] public delegate void GameStarted();
	[Signal] public delegate void GameTilted();
	[Signal] public delegate void PlayerAdded();
	[Signal] public delegate void ScoresUpdated();
	[Signal] public delegate void ScoreEntryEnded();
	#endregion	

	AudioStreamPlayer sfxPlayer;
	Dictionary<string, AudioStream> Sounds = new Dictionary<string, AudioStream>();

	public override void _Ready()
	{
		sfxPlayer = new AudioStreamPlayer();
		Sounds.Add("credit", Load("res://assets/audio/sfx/credit.wav") as AudioStream);
		AddChild(sfxPlayer);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("sw" + CreditButtonNum)) //Coin button. See PinGod.vbs for Standard switches
		{
			sfxPlayer.Stream = Sounds["credit"];
			sfxPlayer.Play();
			AddCredits(1);			
		}
	}

	public void AddCredits(byte amt)
	{
		Credits += amt;
		EmitSignal(nameof(CreditAdded));
	}

	/// <summary>
	/// Adds points to the current player
	/// </summary>
	/// <param name="points"></param>
	public void AddPoints(int points)
	{
		if (Player != null)
		{
			Player.Points += points;
			EmitSignal(nameof(ScoresUpdated));
		}
	}

	/// <summary>
	/// Adds bonus to the current player
	/// </summary>
	/// <param name="points"></param>
	public void AddBonus(int points)
	{
		if (Player != null)
		{
			Player.Bonus += points;
		}
	}

	/// <summary>
	/// Sends a signal game is paused
	/// </summary>
	public void SetGamePaused() => EmitSignal(nameof(GamePaused));

	/// <summary>
	/// Sends a signal game is resumed
	/// </summary>
	public void SetGameResumed() => EmitSignal(nameof(GameResumed));

	/// <summary>
	/// Attempts to start a game.
	/// </summary>
	public bool StartGame()
	{
		Print("global:start game");
		if (IsTilted)
		{
			Print("Cannot start game when game is tilted");
			return false;
		}

		if (!GameInPlay && Credits > 0) //first player start game
		{
			if (!Trough.IsTroughFull()) //return if trough isn't full. TODO: needs debug option to remove check
			{
				Print("Trough not ready. Can't start game with empty trough.");
				return false;
			}

			Players.Clear(); //clear any players from previous game
			GameInPlay = true;

			//remove a credit and add a new player
			Credits--;
			Players.Add(new PlayerBasicGame() { Name = $"P{Players.Count+1}", Points = 0 });
			CurrentPlayerIndex = 0;
			Player = Players[CurrentPlayerIndex];
			Print("signal: player 1 added");
			EmitSignal(nameof(PlayerAdded));
			EmitSignal(nameof(GameStarted));
			return true;
		}
		//game started already, add more players until max
		else if (BallInPlay <= 1 && GameInPlay && Players.Count < MaxPlayers && Credits > 0)
		{
			Credits--;
			Players.Add(new PlayerBasicGame() { Name = $"P{Players.Count+1}", Points = 0 });
			Print($"signal: player added. {Players.Count}");
			EmitSignal(nameof(PlayerAdded));
		}

		return false;
	}

	/// <summary>
	/// Ends the current ball. Changes the player
	/// </summary>
	/// <returns>True if all balls finished, game is finished</returns>
	public bool EndBall()
	{
		EnableFlippers(0);

		if (GameInPlay && Players.Count > 0)
		{
			Print("end of ball. current ball:" + BallInPlay);
			if (Players.Count > 1)
			{
				CurrentPlayerIndex++;
				if(CurrentPlayerIndex+1 > Players.Count)
				{
					CurrentPlayerIndex = 0;
					BallInPlay++;
				}
			}
			else
			{
				BallInPlay++;
			}

			Print("ball in play " + BallInPlay);

			if (BallInPlay > BallsPerGame)
			{
				//signal that ball has ended
				this.EmitSignal(nameof(BallEnded));
				return true;
			}
			else
			{
				//signal that ball has ended
				this.EmitSignal(nameof(BallEnded));				
			}
		}

		return false;
	}

	public void StartNewBall()
	{
		Print("global:starting new ball");
		ResetTilt();
		Player = Players[CurrentPlayerIndex];
		EnableFlippers(1);
		OscService.PulseCoilState(TroughSolenoid);
		EmitSignal(nameof(BallStarted));
	}

	/// <summary>
	/// Reset player warnings and tilt
	/// </summary>
	public void ResetTilt()
	{
		Tiltwarnings = 0;
		IsTilted = false;
	}

	/// <summary>
	/// Game has ended
	/// </summary>
	public void EndOfGame()
	{
		GameInPlay = false;
		IsTilted = false;
		OscService.SetLampState(1, 0);
		//signal that ball has ended
		this.EmitSignal(nameof(GameEnded));
	}
}
