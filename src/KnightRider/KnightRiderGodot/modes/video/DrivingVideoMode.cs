using Godot;
using System.Collections.Generic;

public class PickUpAward
{
	public int Score { get; set; }
	public string Label { get; set; }
}

public class DrivingVideoMode : Node2D
{
	int score = 100;
	int totalScore = 100;
	float[] _lanes = new float[4] { 846, 943, 1045, 1143 };
	List<PickUpAward> _awards = new List<PickUpAward>
	{
		new PickUpAward()
		{
			 Label = "250K", Score = 250000
		},
		new PickUpAward()
		{
			 Label = "500K", Score = 500000
		},
		new PickUpAward()
		{
			 Label = "750K", Score = 750000
		},
		new PickUpAward()
		{
			 Label = "1 MIL", Score = 1000000
		}
	};

	private Label _label;
	private PackedScene _enemyCarScene;
	private PackedScene _pickupScene;
	private PlayerCar _player;
	private bool _gameOver = false;
	[Signal] public delegate void VideoModeEnded(int score);

	public override void _EnterTree()
	{
		_enemyCarScene = GD.Load<PackedScene>("res://modes/video/EnemyCar.tscn");
		_pickupScene = GD.Load<PackedScene>("res://modes/video/Pickup.tscn");
	}

	public override void _Ready()
	{
		_label = GetNode<Label>("CanvasLayer/CenterContainer/Label");
		_player = GetNode<PlayerCar>("CanvasLayer/PlayerCar");
		GetNodeOrNull<PinGodGame>("/root/PinGodGame")?.PlayMusic("kr_video_mode");
		AddEnemy(0); AddEnemy(1, -4900f); AddEnemy(2, -3900f); AddEnemy(3, -1550f);
		AddPickup();
	}

	public override void _Process(float delta)
	{
		if (_gameOver)
		{
			SetProcess(false);
			return;
		}

		if(_player.Position.x > 814 && _player.Position.x < 1169)
		{
			totalScore += score;
			_label.Text = totalScore.ToString("N0");
		}		
	}

	public void AddEnemy(uint randInt, float y = -90f)
	{
		if (!_gameOver)
		{
			var car = _enemyCarScene.Instance<EnemyCar>();
			car.Position = new Vector2(_lanes[randInt], y);
			GetNode("Collidables").CallDeferred("add_child", car);
			car.Connect("tree_exited", this, nameof(EnemyDead));
			car.Connect("PlayerCrashed", this, nameof(OnPlayerCrashed));
		}
	}

	public void AddPickup(uint lane = 1)
	{
		if (!_gameOver)
		{
			var pickup = _pickupScene.Instance<Pickup>();
			pickup.Position = new Vector2(_lanes[lane], -90f);
			var award = _awards[(int)lane];
			pickup.ScorePickedUpValue = award.Score;
			pickup.PickUpText = award.Label;
			GetNode("Collidables").CallDeferred("add_child", pickup);
			
			pickup.Connect("Collected", this, nameof(PickupCollected));
			pickup.Connect("tree_exited", this, nameof(OnPickupDead));
		}		
	}

	public void PickupCollected(int score)
	{
		if (!_gameOver)
		{
			totalScore += score;
		}		
	}

	void OnPickupDead()
	{
		if (!_gameOver)
		{
			var randInt = GD.Randi() % 4 + 0;
			AddPickup(randInt);
		}
	}

	private void EnemyDead()
	{
		if (!_gameOver)
		{
			var randInt = GD.Randi() % 4 + 0;
			randInt = GD.Randi() % 4 + 0;
			AddEnemy(randInt, -(360f * randInt));
		}
	}

	public void OnPlayerCrashed()
	{
		GD.Print("CRASHED");

		_gameOver = true;

		foreach (var item in GetNode("Collidables").GetChildren())
		{
			var ob = item as Node;
			if(ob != null)
			{
				ob.QueueFree();
			}
		}

		_player.QueueFree();
		GetNode<ParallaxBackground>("ParallaxBackground").SetProcess(false);

		EmitSignal(nameof(VideoModeEnded), totalScore);
	}
}
