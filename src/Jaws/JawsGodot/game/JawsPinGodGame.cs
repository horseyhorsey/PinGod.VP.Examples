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
	public int JawsToyState { get; private set; }

	public override void AddAudioStreams()
    {
        base.AddAudioStreams();

		AudioManager.AddSfx("res://assets/audio/sfx/barrel_target.wav", "barrel_target");
		AudioManager.AddSfx("res://assets/audio/sfx/shark_hit_fx.wav", "shark_hit_fx");
		AudioManager.AddSfx("res://assets/audio/sfx/short_fx_03.wav", "short_fx_03");
		AudioManager.AddSfx("res://assets/audio/sfx/bell.wav", "bell");

		//voice 
		AudioManager.AddSfx("res://assets/audio/voice/brody_super_cheer.wav", "brody_super_cheer");
		AudioManager.AddSfx("res://assets/audio/voice/shoot.wav", "shoot");

		//add music for the game. Ogg to autoloop		
		AudioManager.Bgm = "bgmmusic";
		AudioManager.MusicEnabled = true;

		AudioManager.AddMusic("res://assets/audio/music/orca_leaves_port.ogg", "bgmmusic");
		AudioManager.AddMusic("res://assets/audio/music/barrel_start.ogg", "barrel_start");
		AudioManager.AddMusic("res://assets/audio/music/barrels_loop.ogg", "barrels_loop");
		AudioManager.AddMusic("res://assets/audio/music/dundun_1.ogg", "dundun_1");
		AudioManager.AddMusic("res://assets/audio/music/dundun_2.ogg", "dundun_2");
		AudioManager.AddMusic("res://assets/audio/music/orca_mball.ogg", "orca_mball");
		AudioManager.AddMusic("res://assets/audio/music/complete.ogg", "complete");
		AudioManager.AddMusic("res://assets/audio/music/suspense_loop.ogg", "suspense_loop");
		AudioManager.AddMusic("res://assets/audio/music/music_suspense_end.ogg", "music_suspense_end");
		AudioManager.AddMusic("res://assets/audio/music/end_music_final.ogg", "end_music_final");
	}

    internal void EnableJawsToy(bool v)
    {
		LogWarning("todo: enable jaws toy");
    }
}