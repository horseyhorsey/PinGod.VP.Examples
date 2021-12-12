using Godot;

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
		AudioManager.AddMusic("res://assets/audio/music/mus_midnight.ogg", "mus_midnight");
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
		AudioManager.AddMusic("res://assets/audio/music/mus_ingame00.ogg", "mus_ingame00"); //todo: add to plunger lane switch
		AudioManager.SetBgm("mus_main");

		AudioManager.AddSfx("res://assets/audio/sfx/snd_bumper.wav", "snd_bumper");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_down.wav", "snd_down");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_drain.wav", "snd_drain");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_inlane.wav", "snd_inlane");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_return_lanes.wav", "snd_return_lanes");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_kicker.wav", "snd_kicker");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_lowsquirk.wav", "snd_lowsquirk");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_organ.wav", "snd_organ");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_ough.wav", "snd_ough");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_slingshot.wav", "snd_slingshot");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_squirk.wav", "snd_squirk");
		AudioManager.AddSfx("res://assets/audio/sfx/snd_start.wav", "snd_start");


	}	

	
}