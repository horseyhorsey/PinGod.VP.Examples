using Godot;

public class SpecialTargets : PinballTargetsControl
{
	private KnightRiderPlayer _player;
	private AudioStreamPlayer _audio;

	public override void _EnterTree()
	{
		
		this._target_switches = new string[] { "special_0", "special_1", "special_2" };
		this._target_lamps = new string[] { "special_0", "special_1", "special_2" };

		base._EnterTree();

		_audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
	}

	void OnBallStarted()
	{
		_player = pinGod.Player as KnightRiderPlayer;
	}

	public override bool SetTargetComplete(int index)
	{
		var result = base.SetTargetComplete(index);
		pinGod.AddPoints(10000);
		_audio.Play();

		//target was set
		if (result)
		{
			pinGod.AddPoints(Constant.SCORE_STD*2);
		}
		return result;
	}

	public override void TargetsCompleted(bool reset = true)
	{
		base.TargetsCompleted(reset);

		pinGod.LogDebug("enabled special lanes");
		if(_player != null)
		{
			_player.SpecialLanes = new bool[2] { true, true };
			var p = pinGod as KrPinGodGame;
			p.KickbackEnabled = true;

			UpdateLamps();
			if (!p.IsMultiballRunning)
			{
				p.PlayLampshowFlash();
				p.PlayTvScene("kitt_dash", "KICKBACK AND SPECIAL\nLIT", loop: false);
			}						
		}
	}

	public override void UpdateLamps()
	{
		base.UpdateLamps();

		if(_player != null)
		{
			pinGod.SetLampState("special_l", _player.SpecialLanes[0] ? (byte)1 : (byte)0);
			pinGod.SetLampState("special_r", _player.SpecialLanes[1] ? (byte)1 : (byte)0);
		}
	}
}
