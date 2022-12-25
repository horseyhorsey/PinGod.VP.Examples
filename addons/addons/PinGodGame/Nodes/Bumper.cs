using Godot;
using System;

/// <summary>
/// sends BumperHit signal, plays sound and coil if given
/// </summary>
public class Bumper : Node
{
	/// <summary>
	/// Bumper Switch name
	/// </summary>
	[Export] string _SwitchName = string.Empty;

	/// <summary>
	/// Coil name
	/// </summary>
	[Export] string _CoilName = string.Empty;

	/// <summary>
	/// The stream to play when bumper hit
	/// </summary>
	[Export] AudioStream _AudioStream = null;	

	/// <summary>
	/// Emitted on bumper input
	/// </summary>
	/// <param name="name"></param>
	[Signal] delegate void BumperHit(string name);

	private PinGodGame _pinGod;

	/// <summary>
	/// audio player
	/// </summary>
	public AudioStreamPlayer player;

	/// <summary>
	/// Switches off input if no switch available. Sets audio stream
	/// </summary>
    public override void _EnterTree()
    {
		base._EnterTree();

		if (!Engine.EditorHint)
        {			
			_pinGod = GetNode<PinGodGame>("/root/PinGodGame");
			if (_pinGod == null) this.SetProcessInput(false);
			if (string.IsNullOrWhiteSpace(_SwitchName)) this.SetProcessInput(false);

			//update the player stream remove the player from the scene if dev hasn't loaded a stream
			player = GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));
			if (_AudioStream != null)
			{
				player.Stream = _AudioStream;
			}
			//else { this.RemoveChild(player); player.QueueFree(); player = null; }
		}
	}

    internal void SetAudioStream(AudioStream audioStream)
    {
		this._AudioStream = audioStream;
		player.Stream = this._AudioStream;
    }

	/// <summary>
	/// Plays sound and pulses the coil if gameInPlay and isn't tilted
	/// </summary>
	/// <param name="event"></param>
    public override void _Input(InputEvent @event)
	{
		//exit no game in play or tilted
		if (!_pinGod.GameInPlay || _pinGod.IsTilted) return;

		//has switch just been activated?
		if (_pinGod.SwitchOn(_SwitchName, @event))
		{
			//play sound for bumper
			if (_AudioStream != null) { player.Play(); }

			//pulse coil
			if(!string.IsNullOrWhiteSpace(_CoilName)) { _pinGod.SolenoidPulse(_CoilName); }

			//publish hit event
			EmitSignal(nameof(BumperHit), _SwitchName);
		}
		else if (_pinGod.SwitchOff(_SwitchName, @event)) { }
	}
}
