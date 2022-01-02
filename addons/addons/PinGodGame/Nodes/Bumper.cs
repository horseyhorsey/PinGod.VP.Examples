using Godot;

/// <summary>
/// sends BumperHit signal, plays sound and coil if given
/// </summary>
public class Bumper : Node
{
	[Export] string _SwitchName = string.Empty;
	[Export] string _CoilName = string.Empty;
	[Export] AudioStream _AudioStream = null;	
	[Signal] delegate void BumperHit(string name);

	private PinGodGameBase _pinGod;
	public AudioStreamPlayer player;

	public override void _Ready()
	{
		_pinGod = GetNode<PinGodGameBase>("/root/PinGodGame");
		if (_pinGod == null) this.SetProcessInput(false);
		if(string.IsNullOrWhiteSpace(_SwitchName)) this.SetProcessInput(false);

		//update the player stream remove the player from the scene if dev hasn't loaded a stream
		player = GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));
		if (_AudioStream != null)
		{
			player.Stream = _AudioStream;
		}
		else { this.RemoveChild(player); player.QueueFree(); player = null; }		
	}

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
