public class PbPinGodGame : PinGodGame
{
    /// <summary>
    /// override to create our own player type for this game
    /// </summary>
    /// <param name="name"></param>
    public override void CreatePlayer(string name)
    {
        Players.Add(new PlayboyPlayer() { Name = name, Points = 0 });
    }

    public void GetRandomVoiceOrg()
    {

    }

    public void PlaySoundDrain()
    {

    }
}