using Godot;
using System;
using System.Linq;
using static MoonStation.GameGlobals;

public enum ShipState
{
    None,
    Left,
    Right
}

public class MoonLander : Node2D
{
    private static Random _random = new Random();
    private PackedScene _flagInstance;
    private ShipState _shipState;
    bool completed = false;
    Flag[] Flags = new Flag[4];
    private Label scoreLabel;
    private RigidBody2D ship;
    [Signal] public delegate byte FlagEntered(byte value);
    public PinGodGame pinGod { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();
        pinGod = GetNode("/root/PinGodGame") as PinGodGame;
    }

    public override void _Input(InputEvent @event)
    {
        if (ship?.Visible ?? false)
        {
            if (pinGod.SwitchOn("flipper_l", @event))
            {
                _shipState = ShipState.Left;
            }
            else if (pinGod.SwitchOff("flipper_l", @event))
            {
                _shipState = ShipState.None;
            }

            if (pinGod.SwitchOn("flipper_r", @event))
            {
                _shipState = ShipState.Right;
            }
            else if (pinGod.SwitchOff("flipper_r", @event))
            {
                _shipState = ShipState.None;
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (!completed)
        {
            if (_shipState == ShipState.Left)
            {
                var force = ship.AppliedForce.x <= -36f;
                if (!force)
                    ship.AddCentralForce(new Vector2(-9, 0));
            }
            else if (_shipState == ShipState.Right)
            {
                var force = ship.AppliedForce.x >= 36f;
                if (!force)
                    ship.AddCentralForce(new Vector2(9, 0));
            }
        }
        else
        {
            SetPhysicsProcess(false);
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		ship = GetNode("PlayerShip") as RigidBody2D;        
		_flagInstance = GD.Load("moonstation_lander/flag.tscn") as PackedScene;
		scoreLabel = GetNode("Label") as Label;
		scoreLabel.Visible = false;		

		SetUpFlags();
	}
    private void _on_Moon_body_entered(object body)
    {
        pinGod.LogDebug("hit the moon");
        MoonLanderComplete(1);
    }

    private void MoonLanderComplete(byte value)
    {
        if (!completed)
        {
            completed = true;
            scoreLabel.Text = $"{Tr("CRATER_LAND")}{value * 1000}";
            scoreLabel.Visible = true;

            pinGod.AddPoints(value * EXTRA_LARGE_SCORE);

            pinGod.LogDebug("flag value ", value);
            ship.QueueFree();
            foreach (var flag in Flags)
            {
                flag.QueueFree();
            }

            this.SetProcessInput(false);
        }
    }

    void OnFlagEntered(byte value)
    {
        MoonLanderComplete(value);
    }

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
}
