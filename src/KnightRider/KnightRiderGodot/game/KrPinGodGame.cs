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
}