using System;

public class JawsPinGodGame : PinGodGame
{
    /// <summary>
    /// override to create our own player type for this game
    /// </summary>
    /// <param name="name"></param>
    public override void CreatePlayer(string name)
    {
        Players.Add(new JawsPlayer() { Name = name, Points = 0 });
    }

	public JawsPlayer GetJawsPlayer() => Player as JawsPlayer;
	public int JawsToyState { get; set; }

    internal void EnableJawsToy(bool enable)
    {
		SolenoidOn("jaws_toy", (byte)(enable ? 1 : 0));
    }
}