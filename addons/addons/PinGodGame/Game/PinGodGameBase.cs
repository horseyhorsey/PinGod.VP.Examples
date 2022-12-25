using Godot;

/// <summary>
/// Contains generic base signals to emit from the game.
/// </summary>
public abstract class PinGodBase : Node
{
    #region Signals
    /// <summary>
    /// Emitted signal
    /// </summary>
    [Signal] public delegate void BallDrained();
    /// <summary>
    /// Emitted signal
    /// </summary>
    /// <param name="lastBall">Is this last ball?</param>
	[Signal] public delegate void BallEnded(bool lastBall);
    /// <summary>
    /// Emitted signal when a ball is saved
    /// </summary>
	[Signal] public delegate void BallSaved();
    /// <summary>
    /// Emitted signal when ball save is ended
    /// </summary>
	[Signal] public delegate void BallSaveEnded();
    /// <summary>
    /// Emitted signal when ball save starts
    /// </summary>
    [Signal] public delegate void BallSaveStarted();
    /// <summary>
    /// Emitted signal when bonus is ended
    /// </summary>
    [Signal] public delegate void BonusEnded();
    /// <summary>
    /// Emitted signal when a credit is added to game
    /// </summary>
    [Signal] public delegate void CreditAdded();
    /// <summary>
    /// Emitted signal when game ends
    /// </summary>
    [Signal] public delegate void GameEnded();
    /// <summary>
    /// Emitted signal when game is paused
    /// </summary>
    [Signal] public delegate void GamePaused();
    /// <summary>
    /// Emitted signal when game is resumed
    /// </summary>
    [Signal] public delegate void GameResumed();
    /// <summary>
    /// Emitted signal when game starts
    /// </summary>
    [Signal] public delegate void GameStarted();
    /// <summary>
    /// Emitted signal when game is tilted
    /// </summary>
    [Signal] public delegate void GameTilted();
    /// <summary>
    /// Emitted signal when a mode times out
    /// </summary>
    [Signal] public delegate void ModeTimedOut(string title);
    /// <summary>
    /// Emitted signal when multi-ball ends
    /// </summary>
    [Signal] public delegate void MultiBallEnded();
    /// <summary>
    /// Emitted signal when multi-ball starts
    /// </summary>
    [Signal] public delegate void MultiballStarted();
    /// <summary>
    /// Emitted signal when a player is added to the game
    /// </summary>
    [Signal] public delegate void PlayerAdded();
    /// <summary>
    /// Emitted signal when player has finished entering their scores
    /// </summary>
    [Signal] public delegate void ScoreEntryEnded();
    /// <summary>
    /// Emitted signal each time score is updated
    /// </summary>
    [Signal] public delegate void ScoresUpdated();
    /// <summary>
    /// Emitted signal when player enters the service menu
    /// </summary>
    [Signal] public delegate void ServiceMenuEnter();
    /// <summary>
    /// Emitted signal when player exits the service menu
    /// </summary>
	[Signal] public delegate void ServiceMenuExit();
    /// <summary>
    /// Signal sent from memory map if <see cref="GameSettings.VpCommandSwitchId"/> was found while reading states
    /// </summary>
    /// <param name="index"></param>
    [Signal] public delegate void VpCommand(byte index);
    #endregion
}