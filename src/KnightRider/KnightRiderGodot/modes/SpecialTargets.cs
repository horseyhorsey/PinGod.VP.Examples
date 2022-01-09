using Godot;

public class SpecialTargets : PinGodGameMode
{
	private KnightRiderPlayer _player;
	private AudioStreamPlayer _audio;

	public override void _EnterTree()
	{
		base._EnterTree();
		_audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		_audio.Stream = pinGod.AudioManager.Sfx["gun01"];
	}

	protected override void OnBallStarted()
	{
		base.OnBallStarted();
		_player = pinGod.Player as KnightRiderPlayer;
	}

	void _on_PinballTargetsBank_OnTargetActivated(string swName, bool completed)
    {
		pinGod.AddPoints(10000);
		_audio.Play();

		//target was set
		if (completed)
		{
			pinGod.AddPoints(Constant.SCORE_STD * 2);
		}
	}

	void _on_PinballTargetsBank_OnTargetsCompleted()
    {
		pinGod.LogDebug("enabled special lanes");
		if (_player != null)
		{
			_player.SpecialLanes = new bool[2] { true, true };
			var p = pinGod as KrPinGodGame;
			p.KickbackEnabled = true;

			UpdateLamps();
			if (!p.IsMultiballRunning)
			{
				p.PlayLampshowFlash();
				p.PlayTvScene("kitt_dash", Tr("KICKBACK_LIT"), loop: false);
			}
		}
	}

	protected override void UpdateLamps()
	{
		base.UpdateLamps();

		if(_player != null)
		{
			pinGod.SetLampState("special_l", _player.SpecialLanes[0] ? (byte)1 : (byte)0);
			pinGod.SetLampState("special_r", _player.SpecialLanes[1] ? (byte)1 : (byte)0);
		}
	}
}
