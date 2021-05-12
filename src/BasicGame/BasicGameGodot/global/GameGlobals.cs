using Godot;
using System.Collections.Generic;
using static Godot.GD;

/// <summary>
/// Godot singleton to hold Common pinball variables
/// </summary>
public class GameGlobals : Node
{
	#region Public Properties - Standard Pinball / Players
	public static int Credits { get; set; }
	public static int CurrentPlayerIndex { get; private set; }
	public static int BallInPlay { get; private set; }
	public static byte BallsPerGame { get; private set; } = 3;
	public static bool GameInPlay { get; private set; }
	public static Player Player { get; private set; }
	public static List<Player> Players { get; private set; } 
	#endregion

	private static byte MaxPlayers = 4;

	public GameGlobals()
	{
		Players = new List<Player>();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("sw19")) //Start button. See PinGod.vbs for Standard switches
		{
			StartGame();
		}
		if (@event.IsActionPressed("sw2")) //Coin button. See PinGod.vbs for Standard switches
		{
			GameGlobals.Credits++;            
		}
		if (@event.IsActionPressed("sw84"))
		{
			//end the ball in play?
			if (Trough.IsTroughFull && !Trough.BallSaveActive)
			{
				EndOfBall();
			}
		}
	}

	/// <summary>
	/// Adds points to the current player
	/// </summary>
	/// <param name="points"></param>
	internal static void AddPoints(int points)
	{
		if(Player != null)
			Player.Points += points;
	}

	/// <summary>
	/// Attempts to start a game.
	/// </summary>
	private void StartGame()
	{			
		if (!GameInPlay && Credits > 0) //first player start game
		{			
			if (!Trough.IsTroughFull) //return if trough isn't full. TODO: needs debug option to remove check
			{
				Print("Trough not ready. Can't start game with empty trough.");
				return;
			}

			Players.Clear(); //clear any players from previous game
			GameInPlay = true;

			//load the basic game scene
			Print("loading game scene");
			GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://Game.tscn"));

			//remove a credit and add a new player
			Credits--;
			Players.Add(new Player() { Name = $"P{Players.Count+1}", Points = 0 });
			CurrentPlayerIndex = 0;
			Player = Players[CurrentPlayerIndex];
			Print("player added");

			//pulse ball from trough		
			BallInPlay = 1;
			OscService.PulseCoilState(1);
		}
		//game started already, add more players until max
		else if (GameInPlay && Players.Count < MaxPlayers && Credits > 0 && BallInPlay <=1)
		{
			Credits--;
			Players.Add(new Player() { Name = $"P{Players.Count+1}", Points = 0 });
			Print("player added");
		}
	}

	/// <summary>
	/// End of ball. Check if end of game, change the player
	/// </summary>
	private void EndOfBall()
	{
		if (GameInPlay && Players.Count > 0)
		{
			Print("end of ball " + BallInPlay);
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
				EndOfGame();
			}
			else
			{
				Player = Players[CurrentPlayerIndex];
				OscService.PulseCoilState(1);
			}
		}		
	}

	/// <summary>
	/// Game has ended
	/// </summary>
	private void EndOfGame()
	{
		GameInPlay = false;		
		OscService.SetLampState(1, 0);
		Print("game ended. loading attract");
		GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://Attract.tscn"));		
	}
}
