using Godot;
using System.Collections.Generic;

/// <summary>
/// Helper class for audio. <para/>
/// note: if you want the finished signal on audio to work when the file is an ogg, the loop must be unchecked then reimported from the import tab
/// </summary>
public class AudioManager : Node
{
    #region Exports

    /// <summary>
    /// Background music key
    /// </summary>
    [Export] public string Bgm { get; set; }

    [Export] public Dictionary<string, string> MusicAssets { get; private set; } = new Dictionary<string, string>();
    [Export]
    public Dictionary<string, string> SfxAssets { get; private set; } = new Dictionary<string, string>() {
        { "credit" , "res://addons/PinGodGame/assets/audio/sfx/credit.wav"},
        { "tilt" , "res://addons/PinGodGame/assets/audio/sfx/tilt.wav"},
        { "warning" , "res://addons/PinGodGame/assets/audio/sfx/tilt_warning.wav"}
    };
    [Export] public Dictionary<string, string> VoiceAssets { get; private set; } = new Dictionary<string, string>();

    #endregion

    public string CurrentMusic { get; set; }
    public Dictionary<string, AudioStream> Music { get; private set; }

    public bool MusicEnabled { get; set; }

    public AudioStreamPlayer MusicPlayer { get; private set; }

    public Dictionary<string, AudioStream> Sfx { get; private set; }

    public bool SfxEnabled { get; set; }

    public AudioStreamPlayer SfxPlayer { get; private set; }
    public Dictionary<string, AudioStream> Voice { get; private set; }    

    public bool VoiceEnabled { get; set; }

    public AudioStreamPlayer VoicePlayer { get; private set; }

    public override void _EnterTree()
    {
        if (!Engine.EditorHint)
        {
            MusicPlayer = GetNode("MusicPlayer") as AudioStreamPlayer;
            SfxPlayer = GetNode("SfxPlayer") as AudioStreamPlayer;
            VoicePlayer = GetNode("VoicePlayer") as AudioStreamPlayer;

            LoadSoundPckResources();

            Music = new Dictionary<string, AudioStream>();
            Sfx = new Dictionary<string, AudioStream>();
            Voice = new Dictionary<string, AudioStream>();

            foreach (var sfx in SfxAssets)
            {
                AddSfx(sfx.Value, sfx.Key);
            }

            foreach (var vox in VoiceAssets)
            {
                AddVoice(vox.Value, vox.Key);
            }

            foreach (var music in MusicAssets)
            {
                AddMusic(music.Value, music.Key);
            }
        }
    }

    private static void LoadSoundPckResources()
    {
        if (!ProjectSettings.LoadResourcePack("res://pingod.snd.pck"))
        {
            if(!ProjectSettings.LoadResourcePack(System.IO.Path.Combine(Resources.WorkingDirectory, "pingod.snd.pck")))
            {
                Logger.LogInfo("no sound resource pcks found");
            }
            else
            {
                Logger.LogInfo("sound resource pack loaded from " + Resources.WorkingDirectory);
            }
        }
        else
        {
            Logger.LogInfo("sound resource pack loaded local res://");
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
            if (stream != null)
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

    /// <summary>
    /// Adds a Voice resource
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="key"></param>
    public void AddVoice(string resource, string key)
    {
        if (!Voice.ContainsKey(key))
        {
            var stream = GD.Load(resource) as AudioStream;
            if (stream != null)
            {
                Voice.Add(key, stream);
                Logger.LogDebug("Voice added: ", key, resource);
            }
            else { Logger.LogWarning($"Voice add fail: {key}", resource); }
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
            Logger.LogDebug("playing music:", name);
            CurrentMusic = name;
            MusicPlayer.Stream = Music[name];
            MusicPlayer.Play(pos);
            MusicPlayer.Playing = true;
        }
    }

    public void PlayMusic(AudioStream audio, float pos = 0f)
    {
        if (audio == null || !MusicEnabled || Music == null) return;

        MusicPlayer.Stream = audio;
        var name = audio.ResourceName;
        CurrentMusic = name;
        MusicPlayer.Play(pos);
        MusicPlayer.Playing = true;
        Logger.LogDebug("playing music stream: ", name);
    }

    public void PlaySfx(string name, string bus = "Sfx")
    {
        if (!SfxEnabled || string.IsNullOrWhiteSpace(name) || Sfx == null) return;

        if (!Sfx.ContainsKey(name))
            Logger.LogWarning($"play sfx: '{name}' not found");
        else
        {
            SfxPlayer.Stream = Sfx[name];
            SfxPlayer.Bus = bus;
            SfxPlayer.Play();
        }
    }

    public void PlayVoice(string name, string bus = "Voice")
    {
        if (!VoiceEnabled || string.IsNullOrWhiteSpace(name) || Voice == null) return;

        if (!Voice.ContainsKey(name))
            Logger.LogWarning($"play voice: '{name}' not found");
        else
        {
            PlayVoice(Voice[name], bus);
        }
    }

    public void PlayVoice(AudioStream voice, string bus = "Voice")
    {
        if (!VoiceEnabled || Voice == null) return;

        VoicePlayer.Bus = bus;
        VoicePlayer.Stream = voice;
        VoicePlayer.Play();
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

    internal bool IsMusicPlaying() => MusicPlayer.Playing;
    internal void PlayBgm(float pos = 0)
    {
        if (!string.IsNullOrWhiteSpace(Bgm))
            PlayMusic(Bgm, pos);
    }

    internal void SetMusicVolume(float musicVolume)
    {
		Godot.AudioServer.SetBusVolumeDb(1, musicVolume);
    }
}
