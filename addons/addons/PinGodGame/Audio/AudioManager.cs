using Godot;
using System.Collections.Generic;

/// <summary>
/// Helper class for audio. <para/>
/// note: if you want the finished signal on audio to work when the file is an ogg, the loop must be unchecked then reimported from the import tab
/// </summary>
public class AudioManager : Node
{
	/// <summary>
	/// Background music key
	/// </summary>
	[Export] public string Bgm { get; set; }

    public string CurrentMusic { get; private set; }
    public Dictionary<string, AudioStream> Music { get; private set; }
    [Export] public Dictionary<string, string> MusicAssets { get; private set; } = new Dictionary<string, string>();
    [Export] public bool MusicEnabled { get; set; } = true;

    public AudioStreamPlayer MusicPlayer { get; private set; }
    public Dictionary<string, AudioStream> Sfx { get; private set; }
    [Export]
    public Dictionary<string, string> SfxAssets { get; private set; } = new Dictionary<string, string>() {
        { "credit" , "res://addons/PinGodGame/assets/audio/sfx/credit.wav"},
        { "tilt" , "res://addons/PinGodGame/assets/audio/sfx/tilt.wav"},
        { "warning" , "res://addons/PinGodGame/assets/audio/sfx/tilt_warning.wav"}
    };

    [Export] public bool SfxEnabled { get; set; } = true;
    public AudioStreamPlayer SfxPlayer { get; private set; }
	public override void _EnterTree()
	{
        if (!Engine.EditorHint)
        {
			MusicPlayer = GetNode("MusicPlayer") as AudioStreamPlayer;
			SfxPlayer = GetNode("SfxPlayer") as AudioStreamPlayer;

			Music = new Dictionary<string, AudioStream>();
			Sfx = new Dictionary<string, AudioStream>();

			foreach (var sfx in SfxAssets)
            {
				AddSfx(sfx.Value, sfx.Key);
            }

			foreach (var music in MusicAssets)
			{
				AddMusic(music.Value, music.Key);
			}
		}		
	}

	/// <summary>
	/// Adds a music resource
	/// </summary>
	/// <param name="resource"></param>
	/// <param name="key"></param>
	public void AddMusic(string resource, string key)
	{
		if (!Music.ContainsKey(key))
		{
			var stream = GD.Load(resource) as AudioStream;
			if(stream != null)
			{
				Music.Add(key, stream);
				Logger.LogDebug("music added: ", key, resource);
			}
			else { Logger.LogError("music add fail: ", key, resource); }
		}
	}

	/// <summary>
	/// Adds a SFX resource
	/// </summary>
	/// <param name="resource"></param>
	/// <param name="key"></param>
	public void AddSfx(string resource, string key)
	{
		if (!Sfx.ContainsKey(key))
		{
			var stream = GD.Load(resource) as AudioStream;
			if (stream != null)
			{
				Sfx.Add(key, stream);
				Logger.LogDebug("sfx added: ", key, resource);
			}
			else { Logger.LogWarning($"sfx add fail: {key}", resource); }
		}
	}

	public void MusicPlayer_finished()
	{
		Logger.LogDebug($"{MusicPlayer.Stream?.ResourceName} - music player finished");
	}

	public void PauseMusic(bool paused) => MusicPlayer.StreamPaused = paused;

	public void PlayMusic(string name, float pos = 0f)
	{
		if (string.IsNullOrWhiteSpace(name) || !MusicEnabled || Music == null) return;
		if (!Music.ContainsKey(name))
			Logger.LogWarning($"play music: '{name}' not found");
		else
		{
			Logger.LogDebug("playing music:",name);
			CurrentMusic = name;
			MusicPlayer.Stream = Music[name];			
			MusicPlayer.Play(pos);
			MusicPlayer.Playing = true;
		}        
	}

	public void PlaySfx(string name)
	{
		if (string.IsNullOrWhiteSpace(name) || !SfxEnabled || Sfx == null) return;
		if (!Sfx.ContainsKey(name))			
			Logger.LogWarning($"play sfx: '{name}' not found");
		else
		{
			SfxPlayer.Stream = Sfx[name];
			SfxPlayer.Play();
		}
	}

	/// <summary>
	/// Set the BGM flag. Background Music
	/// </summary>
	/// <param name="name"></param>
	public void SetBgm(string name) => Bgm = name;

	/// <summary>
	/// Stops any music playing
	/// </summary>
	/// <returns>The position in secs where stopped</returns>
	public float StopMusic()
	{
		var lastPos = MusicPlayer.GetPlaybackPosition();
		MusicPlayer.Stop();
		return lastPos;
	}

    internal AudioStream GetCurrentMusic() => MusicPlayer.Stream;

    internal void PlayBgm(float pos = 0)
    {
		if (!string.IsNullOrWhiteSpace(Bgm))
			PlayMusic(Bgm, pos);
	}
}
