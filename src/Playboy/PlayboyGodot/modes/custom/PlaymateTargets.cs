using Godot;
using Godot.Collections;

/// <summary>
/// Completing targets advance bonus. TODO: add random sounds
/// </summary>
public class PlaymateTargets : PinballTargetsControl
{
	private Sprite[] pMateSprites;
	private AudioStreamPlayer _audio;
	private Array orgStreams;
	private PlayboyPlayer _player;

	public override void _EnterTree()
	{
		base._EnterTree();

		pMateSprites = new Sprite[] {
			GetNode<Sprite>("PlaymatesBar/0"),
			GetNode<Sprite>("PlaymatesBar/1"),
			GetNode<Sprite>("PlaymatesBar/2"),
			GetNode<Sprite>("PlaymatesBar/3"),
			GetNode<Sprite>("PlaymatesBar/4")
		};

		PlayMateReset();

		_audio = GetNode<AudioStreamPlayer>("Audio");

		orgStreams = new Godot.Collections.Array();
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_ah.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_ahhah.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_breath.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_ohh.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_ugh.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_yes.wav"));
		if(pinGod.Player !=null) _player = pinGod.Player as PlayboyPlayer;
	}

	public override bool CheckTargetsCompleted(int index)
	{
		base.pinGod.LogInfo("playmate completed");

		return base.CheckTargetsCompleted(index);
	}

	public override bool SetTargetComplete(int index)
	{
		pinGod.AddPoints(Game.MINSCORE);		
		var result = base.SetTargetComplete(index);
		if (!result) { }
		else
		{
			var rnd = new Godot.RandomNumberGenerator();
			rnd.Randomize();
			var val = rnd.RandiRange(0, orgStreams.Count-1);
			_audio.Stream = orgStreams[val] as AudioStreamSample;
			_audio.Play();

			_player?.AdvanceBonus(1);
			pinGod.LogInfo("base: advanced bonus - " + _player.BonusTimes);
			UpdateLamps();
		}

		return result;
	}

	public override void TargetsCompleted(bool reset = true)
	{
		if (reset)
		{
			PlayMateReset();
		}

		if(_player != null)
		{			
			_player.PlaymateFeatureComplete += 1;
			if (_player.PlaymateFeatureComplete == 1) _player.ExtraBallLit = true;
			else if (_player.PlaymateFeatureComplete == 2) _player.LeftSpecialLit = true;
		}
		else { pinGod.LogWarning("No player found"); }

		base.TargetsCompleted(reset);
	}

	private void PlayMateReset()
	{
		foreach (var pmate in pMateSprites)
		{
			pmate.Visible = false;
		}
	}

	public void OnBallDrained()
	{
		pinGod.LogInfo("playmates: ball drained");
		this.ResetTargets();
		_player.KeyFeatureComplete = 0;		
	}

	public void OnBallStarted() => UpdateLamps();

	public override void UpdateLamps()
	{
		base.UpdateLamps();

		//update lamps on the screen
		for (int i = 0; i < _targetValues.Length; i++)
		{
			pMateSprites[i].Visible = _targetValues[i];
		}
	}
}
