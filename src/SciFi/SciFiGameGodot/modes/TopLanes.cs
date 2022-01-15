using Godot;

public class TopLanes : PinGodGameMode
{
	private SciFiPinGodGame pinGodSciFi;
	private SciFiPlayer player;

	#region Exports
	[Export] bool _logging_enabled = false;
	#endregion

	public override void _EnterTree()
	{
		base._EnterTree();

		pinGodSciFi = GetNode("/root/PinGodGame") as SciFiPinGodGame;

		if (_logging_enabled)
			pinGod.LogInfo("top_lanes: enter tree");
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

    protected override void OnBallStarted()
    {
        player = pinGodSciFi.GetSciFiPlayer();

        base.OnBallStarted();
    }

    protected override void UpdateLamps()
    {
        base.UpdateLamps();
        UpdateBonusLamps();
    }

    void _on_PinballLanesNode_LaneCompleted(string swName, bool complete)
    {
		if (complete)
		{
			pinGod.AddPoints(1000);
			pinGodSciFi.GetSciFiPlayer().AddAlienBonus(1);
			pinGod.PlaySfx("bonus_advance");
		}
	}

	void _on_PinballLanesNode_LanesCompleted()
    {
		if (player.AddBonusMultiplier())
		{
			pinGod.PlaySfx("multiplier_award");
			UpdateBonusLamps();
		}
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
