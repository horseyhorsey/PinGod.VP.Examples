using Godot;

/// <summary>
/// Custom class of <see cref="PinGodGame"/> to override when a player is added
/// </summary>
public class CustomPinGodGame : PinGodGame
{
    /// <summary>
    /// override to create our own player type for this game
    /// </summary>
    /// <param name="name"></param>
    public override void CreatePlayer(string name)
    {
        Players.Add(new BasicGamePlayer() { Name = name, Points = 0 });
    }

    /// <summary>
    /// Logs when this class is setup, nothing more.
    /// </summary>
    public override void Setup()
    {
        base.Setup();
        LogInfo("setup custom game finished");
    }
}