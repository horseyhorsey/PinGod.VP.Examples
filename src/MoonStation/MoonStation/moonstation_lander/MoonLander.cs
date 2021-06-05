using Godot;
using System;
using System.Linq;

public class MoonLander : Node2D
{
	private RigidBody2D ship;
	private Label scoreLabel;
	private PackedScene _flagInstance;
	[Signal] public delegate byte FlagEntered(byte value);

	Flag[] Flags = new Flag[4];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ship = GetNode("PlayerShip") as RigidBody2D;        
		_flagInstance = GD.Load("moonstation_lander/flag.tscn") as PackedScene;
		scoreLabel = GetNode("Label") as Label;
		scoreLabel.Visible = false;

		PinGodGame = GetNode("/root/PinGodGame") as PinGodGame;

		SetUpFlags();
	}

	private static Random _random = new Random();

	private void SetUpFlags()
	{
		var array = Enumerable.Range(_random.Next(4), 4).ToArray();

		var flag = _flagInstance.Instance() as Flag;
		flag.RotationDegrees = -35.5f; flag.Position = new Vector2(36.5f, 301.1f);
		flag.SetMultiplier((byte)array[0]);
		Flags[0] = flag;


		flag = _flagInstance.Instance() as Flag;
		flag.RotationDegrees = -17.6f; flag.Position = new Vector2(133.5f, 249.4f);
		flag.SetMultiplier((byte)array[1]);
		Flags[1] = flag;

		flag = _flagInstance.Instance() as Flag;
		flag.RotationDegrees = 16.8f; flag.Position = new Vector2(357.81f, 241.9f);
		flag.SetMultiplier((byte)array[2]);
		Flags[2] = flag;

		flag = _flagInstance.Instance() as Flag;
		flag.RotationDegrees = 30.4f; flag.Position = new Vector2(467.43f, 296.08f);
		flag.SetMultiplier((byte)array[3]);
		Flags[3] = flag;

		for (int i = 0; i < Flags.Length; i++)
		{
			AddChild(Flags[i]);
		}

		Connect("FlagEntered", this, "OnFlagEntered");
	}

	bool completed = false;

	public PinGodGame PinGodGame { get; private set; }

	void OnFlagEntered(byte value)
	{
		MoonLanderComplete(value);
	}

	private void _on_Moon_body_entered(object body)
	{
		GD.Print("hit the moon");
		MoonLanderComplete(1);
	}

	private void MoonLanderComplete(byte value)
	{
		if (!completed)
		{
			completed = true;
			scoreLabel.Text = $"CRATER LANDING\r\nSCORES\r\n{value * 1000}";
			scoreLabel.Visible = true;

			PinGodGame.AddPoints(value * 1000);

			GD.Print("flag value ", value);
			ship.QueueFree();
			foreach (var flag in Flags)
			{
				flag.QueueFree();
			}

			this.SetProcessInput(false);
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (ship?.Visible ?? false)
		{
			if (PinGodGame.SwitchOn("flipper_l", @event))
			{
				var force = ship.AppliedForce.x <= -36f;
				if (!force)
					ship.AddCentralForce(new Vector2(-9, 0));
			}
			if (PinGodGame.SwitchOn("flipper_r", @event))
			{
				var force = ship.AppliedForce.x >= 36f;
				if (!force)
					ship.AddCentralForce(new Vector2(9, 0));
			}
		}
	}
}
