using Godot;

public class Pickup : Area2D
{
	float _offScreenPosY = 1080f;
	public int ScorePickedUpValue = 250000;
	public string PickUpText = "250K";

	[Export] int _speed = 2;
	[Signal] delegate void Collected(int value);
	private Tween _tween;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var label = GetNode<Label>("Label");
		label.Text= PickUpText;
		_tween = GetNode<Tween>("Tween");
		_tween.InterpolateProperty(label, "rect_scale:x", 1, 2, 1);
		_tween.InterpolateProperty(label, "rect_scale:y", 1, 2, 1);
		_tween.InterpolateProperty(label, "modulate", label.Modulate, Colors.Transparent, 1);

		var _pinGod = GetNode<PinGodGame>("/root/PinGodGame");
		GetNode<AudioStreamPlayer>("AudioStreamPlayer").Stream = _pinGod.AudioManager.Sfx["Beep"];
	}

	public override void _Process(float delta)
	{
		this.Position = new Vector2(Position.x, Position.y + _speed);
		if (Position.y > _offScreenPosY)
		{
			QueueFree();
		}
	}

	private bool collected = false;
	private void _on_Pickup_body_entered(object body)
	{
		if (!collected)
		{
			GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play();
			_tween.Start();
			collected = true;
			EmitSignal(nameof(Collected), ScorePickedUpValue);
		}
	}
}
