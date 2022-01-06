using Godot;

public class CustomPinGodGame : PinGodGame
{
    /// <summary>
    /// override to create our own player type for this game
    /// </summary>
    /// <param name="name"></param>
    public override void CreatePlayer(string name)
    {
        Players.Add(new BasicDisplayPlayer() { Name = name, Points = 0 });
    }

    public override void Setup()
    {
        base.Setup();
        LogInfo("setup custom game finished");
    }
}