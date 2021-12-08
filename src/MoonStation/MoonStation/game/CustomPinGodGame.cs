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

    public override void AddAudioStreams()
    {
        base.AddAudioStreams();

        //add sfx for the game        
        AudioManager.AddSfx("res://assets/audio/sfx/Laser_Shoot3-horsepin.wav", "spinner");
        AudioManager.AddSfx("res://assets/audio/sfx/dropT-horsepin.wav", "drops");
        AudioManager.AddSfx("res://assets/audio/sfx/dropTComplete-horsepin.wav", "drops_complete");
        AudioManager.AddSfx("res://assets/audio/sfx/craterhit-horsepin.wav", "crater");

        //add music for the game
        AudioManager.AddMusic("res://assets/audio/music/ms-music-dnb.ogg", "dnb");
        AudioManager.AddMusic("res://assets/audio/music/ms-music-techno.ogg", "techno");
    }
}