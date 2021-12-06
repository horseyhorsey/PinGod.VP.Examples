using Godot;

public class Bumper : Node
{
	[Export] string _SwitchName;
	[Export] string _CoilName;
	[Export] AudioStream _AudioStream;	
	[Signal] delegate void BumperHit(string name);

	private PinGodGameBase _pinGod;
	private AudioStreamPlayer player;

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
			if (player != null) { player.Play(); }
			//pulse coil
			if(!string.IsNullOrWhiteSpace(_CoilName)) { _pinGod.SolenoidPulse(_CoilName); }
			//publish hit event
			EmitSignal(nameof(BumperHit), _SwitchName);
		}
		else if (_pinGod.SwitchOff(_SwitchName, @event)) { }
	}
}
