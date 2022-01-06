using Godot;
using Godot.Collections;

/// <summary>
/// Completing targets advance bonus. TODO: add random sounds
/// </summary>
public class PlaymateTargets : PinGodGameMode
{
    private AudioStreamPlayer _audio;
    private Game _game;
    private Label _label;
    private PlayboyPlayer _player;
    private PinballTargetsBank _targets;
    private Array orgStreams;
    private Sprite[] pMateSprites;

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

		_label = GetNode<Label>("PlaymatesBar/TimesCompleteLabel");

		_game = GetParent().GetParent<Game>();
		_targets = GetNode<PinballTargetsBank>(nameof(PinballTargetsBank));

		orgStreams = new Godot.Collections.Array();
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_ah.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_ahhah.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_breath.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_ohh.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_ugh.wav"));
		orgStreams.Add(GD.Load<AudioStreamSample>("res://assets/audio/voice/org_yes.wav"));
		if(pinGod.Player !=null) _player = pinGod.Player as PlayboyPlayer;
	}

    protected override void OnBallDrained()
    {
        pinGod.LogInfo("playmates: ball drained");
        _targets.ResetTargets();
        _player.KeyFeatureComplete = 0;
    }

    protected override void OnBallStarted()
    {
        SetValueLabel();
        UpdateLamps();
    }

    protected override void UpdateLamps()
    {
        base.UpdateLamps();

        //update lamps on the screen
        for (int i = 0; i < _targets._targetValues.Length; i++)
        {
            pMateSprites[i].Visible = _targets._targetValues[i];
        }
    }

    void _on_PinballTargetsBank_OnTargetActivated(string swName, bool complete)
    {
		pinGod.AddPoints(Game.MINSCORE);

		if (!complete) { }
		else
		{
			var rnd = new Godot.RandomNumberGenerator();
			rnd.Randomize();
			var val = rnd.RandiRange(0, orgStreams.Count - 1);
			_audio.Stream = orgStreams[val] as AudioStreamSample;
			_audio.Play();

			_player?.AdvanceBonus(1);
			pinGod.LogInfo(swName + ": playmate target complete");
			_game.UpdateLamps();
		}
	}

	void _on_PinballTargetsBank_OnTargetsCompleted()
    {
		base.pinGod.LogInfo("playmates completed");

		PlayMateReset();

		if (_player != null)
		{
			pinGod.PlaySfx("horse-disco-laser");
			_player.PlaymateFeatureComplete++;

			if (_player.PlaymateFeatureComplete == 1)
			{
				pinGod.AddPoints(10000);
				_player.ExtraBallLit = true;
				pinGod.LogInfo("playmate feature extra ball lit");
			}
			else if (_player.PlaymateFeatureComplete == 2)
			{
				pinGod.AddPoints(20000);
				_player.LeftSpecialLit = true;
				pinGod.LogInfo("20k, playmate feature left special lit");
			}
			else
			{
				pinGod.AddPoints(50000);
			}

			SetValueLabel();
		}
		else { pinGod.LogWarning("No player found"); }

		_targets.TargetsCompleted(true);

		_game.UpdateLamps();
	}

    private void PlayMateReset()
    {
        foreach (var pmate in pMateSprites)
        {
            pmate.Visible = false;
        }
    }

    private void SetValueLabel()
    {
		if(_player.PlaymateFeatureComplete < 1)
			_label.Text = "10K";
		else if (_player.PlaymateFeatureComplete == 1)
			_label.Text = "20K";
		else _label.Text = "50K";

	}
}
