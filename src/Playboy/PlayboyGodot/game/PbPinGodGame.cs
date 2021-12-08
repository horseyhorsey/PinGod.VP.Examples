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

    public override void AddAudioStreams()
    {
        base.AddAudioStreams();
        //add music for the game. Ogg to autoloop
        AudioManager.AddMusic("res://assets/audio/music/cook_loop1.ogg", "cook_loop_1");
        AudioManager.AddMusic("res://assets/audio/music/cook_loop2.ogg", "cook_loop_2");
    }

    public void GetRandomVoiceOrg()
    {

    }

    public void PlaySoundDrain()
    {

    }
}