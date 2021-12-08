using Godot;

public class KrPinGodGame : PinGodGame
{
    [Signal] public delegate void ChangeTvScene(string name, string label = "", float clearDelay = 2f, bool loop = true);

    [Signal] public delegate void StartKarrTimer(float time = 30f);

    internal void PlayLampshow() => SolenoidPulse("show_random");
    internal void PlayLampshowFlash() => SolenoidPulse("show_flash");

    public bool KickbackEnabled { get; internal set; }

    /// <summary>
    /// override to create our own player type for this game
    /// </summary>
    /// <param name="name"></param>
    public override void CreatePlayer(string name)
    {
        Players.Add(new KnightRiderPlayer() { Name = name, Points = 0 });
    }

    internal void AwardBillion()
    {
        AddPoints(1000000000);
        var p = Player as KnightRiderPlayer;
        p.ResetCompletedModes();
        PlayLampshow();
        PlayLampshowFlash();
        LogInfo("billion shot awarded, modes reset");
        PlaySfx("mike_laughs");
    }

	internal void EnableTopFlippers(byte enable)
	{
		SolenoidOn("flippers_top", enable);
	}

	internal bool EnableTruckRamp()
	{
		var player = Player as KnightRiderPlayer;
		if (player != null)
		{

			if (IsMultiballRunning)
			{
				if (!SwitchOn("truck_down"))
				{
					SolenoidPulse("truck_ramp");
					return true;
				}
			}

			if (player.IsKarrRunning)
			{
				if (SwitchOn("truck_down"))
				{
					SolenoidPulse("truck_ramp");
					return false;
				}
			}

			if (!player.IsTruckRampReady)
			{
				if (SwitchOn("truck_down"))
				{
					SolenoidPulse("truck_ramp");
					return false;
				}
			}
			else
			{
				if (SwitchOn("truck_up"))
				{
					SolenoidPulse("truck_ramp");
				}
				return true;
			}
		}

		return false;
	}

	public override void EndOfGame()
	{
		base.EndOfGame();
		SolenoidOn("truck_diverter", 1); //empty an balls in the lock
	}

	internal void PlayTvScene(string sceneName, string label = "", float clearDelay = 2f, bool loop = true)
	{
		EmitSignal(nameof(ChangeTvScene), sceneName, label, clearDelay, loop);
	}

	internal int AwardJackpot(bool isTruckShot = false)
	{
		var balls = _trough.BallsInPlay();
		var score = 0;
		var p = Player as KnightRiderPlayer;
		switch (balls)
		{
			case 1:
				score = 75000;
				break;
			case 2:
				score = isTruckShot ? 5000000 : 2500000;
				break;
			case 3:
				score = isTruckShot ? 7500000 : 5000000;
				break;
			case 4:
				score = isTruckShot ? 10000000 : 7500000;
				break;
			default:
				break;
		}

		score = score + p.JackpotAdded;
		AddPoints(score);
		PlayLampshow();
		PlayLampshowFlash();
		PlayTvScene("kitt_bonnet", $"JACKPOT {score.ToScoreString()}", loop: false);
		LogInfo($"jackpot {score}");
		return score;
	}

    public override void AddAudioStreams()
    {
        base.AddAudioStreams();
		AudioManager.AddSfx("res://assets/audio/voice/devon/DEVONwelcomeaboard.wav", "dev_welcome");
		AudioManager.AddSfx("res://assets/audio/sfx/KRfx08.wav", "krfx08");
		AudioManager.AddSfx("res://assets/audio/sfx/gun02.wav", "gun02");
		AudioManager.AddSfx("res://assets/audio/sfx/gun03.wav", "Gun03");
		AudioManager.AddSfx("res://assets/audio/voice/mike/Micheal07.wav", "get_out");
		AudioManager.AddSfx("res://assets/audio/sfx/explosion01.wav", "explosion");
		AudioManager.AddSfx("res://assets/audio/voice/mike/MichealLaughs.wav", "mike_laughs");
		AudioManager.AddSfx("res://assets/audio/voice/devon/DEVONhighscore.wav", "DEVONhighscore");
		AudioManager.AddSfx("res://assets/audio/voice/kitt/KITT08.wav", "KITT08");
		AudioManager.AddSfx("res://assets/audio/voice/kitt/micheal.wav", "micheal");

		//add music for the game. Ogg to autoloop
		AudioManager.AddMusic("res://assets/audio/music/KRsuspense.ogg", "KRsuspense");
		AudioManager.AddMusic("res://assets/audio/music/kr_gameplay.ogg", "kr_gameplay");
		AudioManager.AddMusic("res://assets/audio/music/KRthemeEnd.ogg", "kr_theme_end");
		AudioManager.AddMusic("res://assets/audio/music/KRtheme.ogg", "KRtheme");
		AudioManager.AddMusic("res://assets/audio/music/KR TVintro.ogg", "KRTVintro");
		AudioManager.AddMusic("res://assets/audio/music/BoogieHiScore.ogg", "boogie");
	}
}