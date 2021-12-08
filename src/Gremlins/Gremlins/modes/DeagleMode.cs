using Godot;
using System;

public class DeagleMode : PinballTargetsControl 
{
	[Export] Godot.Collections.Array<AudioStream> _streams = new Godot.Collections.Array<AudioStream>();

	private Game _game;
	private Timer _timer;
	private AudioStreamPlayer _player;
	private AudioStreamPlayer _discSound;
	float _deagleTotalTime = 10;
	float _deagleElapsedTime = 10;

	public override void _Ready()
	{
		base._Ready();

		_game = GetParent().GetParent() as Game;
		_timer = GetNode<Timer>(nameof(Timer));
		_player = GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));
		_discSound = GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer)+"2");
	}

	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay || pinGod.IsTilted) return;
		base._Input(@event);

		if(pinGod.SwitchOn("ramp_r", @event))
		{
			ProcessRightRamp();
		}
	}

	private void ProcessRightRamp()
	{
		pinGod.AddPoints(50000);

		if (_timer.IsStopped())
		{
			//todo: spider mode
			//pinGod.PlaySfx("daffy");
		}
		else
		{
			AwardDeagleJackpot();
		}
	}

	private void AwardDeagleJackpot()
	{
		_timer.Stop();
		_game._player.DeaglesCompleted++;
		var award = 550000 + (500000 * _game._player.DeaglesCompleted);
		pinGod.AddPoints(award);

		//todo: show deagle completed and score, 2.3 delay
		_player.Stream = _streams[1];
		_player.Play();

		ResetDeagle();
	}

	public override bool SetTargetComplete(int index)
	{
		pinGod.AddPoints(2250);

		//skip targets if mode running
		if (_game.ModeRunning) return false;

		return base.SetTargetComplete(index);
	}

	public override void TargetsCompleted(bool reset = true)
	{
		base.TargetsCompleted(false);

		pinGod.LogDebug("deagle targets complete");
		StartDeagleMode();
	}

	private void StartDeagleMode()
	{		
		pinGod.LogDebug("deagle mode started");
		if (_timer.IsStopped())
		{
			_timer.Start();
			_deagleElapsedTime = _deagleTotalTime;
			pinGod.SolenoidOn("deagle_disc", 1);
			_discSound.Play();
		}			
	}

	private void ProcessDeagleTarget(int index)
	{
		
	}

	private void _on_Timer_timeout()
	{		
		if(_deagleElapsedTime <= 0)
		{
			_player.Stream = _streams[3]; // gremlinNaaahhh
			_player.Play();

			_timer.Stop();			
			ResetDeagle();
		}
		else if(_deagleElapsedTime == 4.0f)
		{
			//play random voice for deagle
			GD.Randomize();
			var ranIndex = (int)GD.RandRange(4, 7);
			_player.Stream = _streams[ranIndex];
			_player.Play();
		}

		_deagleElapsedTime--;
	}

	void OnBallDrained()
	{
		ResetDeagle();
	}

	private void ResetDeagle()
	{
		pinGod.SolenoidOn("deagle_disc", 0);
		_discSound.Stop();
		this.ResetTargets();		
	}
}

