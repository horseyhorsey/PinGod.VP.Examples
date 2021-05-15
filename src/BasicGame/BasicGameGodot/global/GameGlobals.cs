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
	public static int BallInPlay { get; set; }
	public static byte BallsPerGame { get; private set; } = 3;
	public static bool GameInPlay { get; private set; }
	public static Player Player { get; private set; }
	public static List<Player> Players { get; private set; }
	#endregion

	#region Signals
	[Signal] public delegate void BallEnded();
	[Signal] public delegate void CreditAdded();
	[Signal] public delegate void GameEnded();
	[Signal] public delegate void GamePaused();
	[Signal] public delegate void GameResumed();
	[Signal] public delegate void GameStarted();
	[Signal] public delegate void PlayerAdded();
	#endregion

	private static byte MaxPlayers = 4;

	public GameGlobals()
	{
		Players = new List<Player>();
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
	internal static void AddPoints(int points)
	{
		if(Player != null)
			Player.Points += points;
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
			Players.Add(new Player() { Name = $"P{Players.Count+1}", Points = 0 });
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
			Players.Add(new Player() { Name = $"P{Players.Count+1}", Points = 0 });
			Print($"signal: player added. {Players.Count}");
			EmitSignal(nameof(PlayerAdded));
		}

		return false;
	}

	/// <summary>
	/// End of ball. Check if end of game, change the player
	/// </summary>
	/// <returns>True if all balls finished, game is finished</returns>
	public bool EndOfBall()
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
				return true;
			}
			else
			{
				//signal that ball has ended
				this.EmitSignal(nameof(BallEnded));
				Player = Players[CurrentPlayerIndex];
				OscService.PulseCoilState(1);
			}
		}

		return false;
	}

	/// <summary>
	/// Game has ended
	/// </summary>
	public void EndOfGame()
	{
		//signal that ball has ended
		this.EmitSignal(nameof(GameEnded));
		GameInPlay = false;
		OscService.SetLampState(1, 0);
	}
}
