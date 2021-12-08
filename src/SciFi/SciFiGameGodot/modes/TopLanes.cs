using Godot;

public class TopLanes : PinballSwitchLanesNode
{
	bool[] _lanesCompleted;
	private SciFiPinGodGame pinGod;
	private SciFiPlayer player;
	[Signal] delegate void LanesCompleted();
	#region Exports
	[Export] bool _logging_enabled = false;
	[Export] bool _reset_on_ball_started = true;
	#endregion

	public override void _EnterTree()
	{
		base._EnterTree();

		pinGod = GetNode("/root/PinGodGame") as SciFiPinGodGame;
		//rest the targets on new ball by listening for BallStarted
		ResetLanesCompleted();

		if (_logging_enabled)
			GD.Print("top_lanes: enter tree");
	}
	/// <summary>
	/// Gets switch handler events for the given switches. Flippers change lanes
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGod.IsTilted) return;
		if (!pinGod.GameInPlay) return;
	}

	public override bool LaneSwitchActivated(int i)
	{
		var result = base.LaneSwitchActivated(i);
		if (result)
		{
			pinGod.AddPoints(1000);
			pinGod.GetSciFiPlayer().AddAlienBonus(1);
			pinGod.PlaySfx("bonus_advance");
		}
		return result;
	}

	public override bool CheckLanes()
	{
		var complete = base.CheckLanes();
		if (complete)
		{
			if (player.AddBonusMultiplier())
			{
				pinGod.PlaySfx("multiplier_award");
				UpdateBonusLamps();
			}
		}
		return complete;
	}

	void OnBallStarted()
	{
		player = pinGod.GetSciFiPlayer();
		if (_reset_on_ball_started)
		{
			ResetLanesCompleted();
		}
	}

	public override void UpdateLamps()
	{
		base.UpdateLamps();
		UpdateBonusLamps();
	}

	private void UpdateBonusLamps()
	{
		if (player.BonusMultiplier == 1)
		{
			pinGod.SetLampState("bonus_2x", 0); pinGod.SetLampState("bonus_3x", 0);
			pinGod.SetLampState("bonus_4x", 0); pinGod.SetLampState("bonus_5x", 0);
			pinGod.SetLampState("bonus_6x", 0);
		}
		else
		{
			var b = player.BonusMultiplier;
			if (b >= 2) pinGod.SetLampState("bonus_2x", 1);
			if (b >= 3) pinGod.SetLampState("bonus_3x", 1);
			if (b >= 4) pinGod.SetLampState("bonus_4x", 1);
			if (b >= 5) pinGod.SetLampState("bonus_5x", 1);
			if (b >= 6) pinGod.SetLampState("bonus_6x", 1);
		}
	}
}
