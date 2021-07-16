using Godot;
using System.Collections.Generic;

public class AudioManager : Node
{    
	public Dictionary<string, AudioStream> Music { get; private set; }
	public Dictionary<string, AudioStream> Sfx { get; private set; }
	public AudioStreamPlayer MusicPlayer { get; private set; }
	public AudioStreamPlayer SfxPlayer { get; private set; }

	/// <summary>
	/// Background music key
	/// </summary>
	public string Bgm { get; set; }
	public string CurrentMusic { get; private set; }
	public bool MusicEnabled { get; set; } = true;
	public bool SfxEnabled { get; set; } = true;

	public AudioManager()
	{        
		Music = new Dictionary<string, AudioStream>();
		Sfx = new Dictionary<string, AudioStream>();		
	}

	public override void _EnterTree()
	{
		MusicPlayer = GetNode("MusicPlayer") as AudioStreamPlayer;
		SfxPlayer = GetNode("SfxPlayer") as AudioStreamPlayer;		
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
		if (!Music.ContainsKey(key))
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

	internal void PlayBgm(float pos = 0)
	{
		if (!string.IsNullOrWhiteSpace(Bgm))
			PlayMusic(Bgm, pos);
	}
}
