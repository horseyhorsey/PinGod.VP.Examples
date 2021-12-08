public class CustomPinGodGame : PinGodGame
{
	/// <summary>
	/// override to create our own player type for this game
	/// </summary>
	/// <param name="name"></param>
	public override void CreatePlayer(string name)
    {
        Players.Add(new NightmarePlayer() { Name = name, Points = 0 });
    }

	public override void AddAudioStreams()
    {
        base.AddAudioStreams();

		AudioManager.SfxEnabled = true;
		AudioManager.MusicEnabled = true;

		//add music for the game. Ogg to autoloop		
		AudioManager.AddMusic("res://assets/audio/music/mus_100k.ogg", "mus_100k");
		AudioManager.AddMusic("res://assets/audio/music/mus_ballready.ogg", "mus_ballready");
		AudioManager.AddMusic("res://assets/audio/music/mus_main.ogg", "mus_main");
		AudioManager.AddMusic("res://assets/audio/music/mus_leftramp.ogg", "mus_leftramp");
		AudioManager.AddMusic("res://assets/audio/music/mus_rampmillion.ogg", "mus_rampmillion");
		AudioManager.AddMusic("res://assets/audio/music/mus_extrahour.ogg", "mus_extrahour");
		AudioManager.AddMusic("res://assets/audio/music/mus_rightramp.ogg", "mus_rightramp");
		AudioManager.AddMusic("res://assets/audio/music/mus_rightloop.ogg", "mus_rightloop");
		AudioManager.AddMusic("res://assets/audio/music/mus_rightorbitcombo.ogg", "mus_rightorbitcombo");
		AudioManager.AddMusic("res://assets/audio/music/mus_alltriangles.ogg", "mus_alltriangles");
		AudioManager.AddMusic("res://assets/audio/music/mus_extraball.ogg", "mus_extraball");
		AudioManager.AddMusic("res://assets/audio/music/mus_jackpot.ogg", "mus_jackpot");
		AudioManager.AddMusic("res://assets/audio/music/mus_spinmillion.ogg", "mus_spinmillion");
		AudioManager.AddMusic("res://assets/audio/music/mus_raisingjackpot.ogg", "mus_raisingjackpot");
		AudioManager.AddMusic("res://assets/audio/music/mus_bonusmultiply.ogg", "mus_bonusmultiply");
		AudioManager.AddMusic("res://assets/audio/music/mus_advancecrossstack.ogg", "mus_advancecrossstack");
		AudioManager.SetBgm("mus_main");
	}
}