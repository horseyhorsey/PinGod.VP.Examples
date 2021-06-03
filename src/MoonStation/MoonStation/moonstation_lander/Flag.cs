using Godot;

public class Flag : Area2D
{
	[Export] byte _multiplier;
	private Node2D lander;

	public override void _Ready()
	{
		lander = GetParent() as Node2D;
	}

	public byte GetMultiplier() => _multiplier;
	public void SetMultiplier(byte value) 
	{ 
		_multiplier = value;
		(GetNode("Label") as Label).Text = $"{value} X";
	}

	private void _on_Flag_body_entered(object body) => lander.EmitSignal("FlagEntered", this._multiplier);
}
