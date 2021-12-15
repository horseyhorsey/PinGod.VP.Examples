public class NightmarePinGodGame : PinGodGame
{	

	/// <summary>
	/// override to create our own player type for this game
	/// </summary>
	/// <param name="name"></param>
	public override void CreatePlayer(string name)
    {
        Players.Add(new NightmarePlayer() { Name = name, Points = 0 });
		PlaySfx("snd_start");
    }

	public NightmarePlayer GetPlayer() => Player as NightmarePlayer;

    public override bool StartGame()
    {
        return base.StartGame();
    }

}