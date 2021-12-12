using Godot;

public class RightTargetsMode : PinballTargetsControl
{
	private Game game;
	private NightmarePlayer player;
	public override void _EnterTree()
	{
		base._EnterTree();
		//get game to resume music
		game = GetParent().GetParent() as Game;

		//hide this mode, still process the switches
		Clear();
	}
	/// <summary>
	/// Just processes the target switches in the base
	/// </summary>
	/// <param name="event"></param>
	public override void _Input(InputEvent @event)
	{
		if (pinGod.IsTilted) return;
		if (!pinGod.GameInPlay) return; //disable when running scene on its own to test input

		base._Input(@event);
	}
	/// <summary>
	/// Advances the cross value for player if less than 6
	/// </summary>
	public void AdvanceCrossValue()
	{
		if (player != null)
		{
			player.CrossValue++;
			pinGod.LogDebug("advancing cross value to ", player.CrossValue);
			if (player.CrossValue < 6)
			{
				var currentCross = player.CrossStack[player.CrossValue - 1];
				if (currentCross == 0)
				{
					player.CrossStack[player.CrossValue - 1] = 2;
					pinGod.AddBonus(NightmareConstants.SMALL_SCORE / 2);
				}

				//get game stop music and resume
				if (game != null)
				{
					game.PlayThenResumeMusic("mus_advancecrossstack", 2.0f);
					//run UpdateLamps in groups marked as Mode
					pinGod.UpdateLamps(game?.GetTree());
				}
				else GD.PushWarning("no game object found to use timer and resume music");

				pinGod.LogInfo("Targets: advance cross value", player.CrossValue);				
			}
		}
		else Logger.LogWarning("No player found");

		game.OnDisplayMessage($"CROSS ADVANCED\n{player?.CrossValue ?? 1}", 2f);
	}

	/// <summary>
	/// Adds score and bonus and checks if complete
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public override bool CheckTargetsCompleted(int index)
	{
		var completed = base.CheckTargetsCompleted(index);
		if (!completed)
			UpdateLamps();
		return completed;
	}

	public void Clear() => this.Visible = false;

	/// <summary>
	/// reset cross value and targets
	/// </summary>
	public void OnBallStarted()
	{
		player = ((NightmarePinGodGame)pinGod).GetPlayer();		
		ResetTargets();
		UpdateLamps();
	}

	/// <summary>
	/// Advances cross value, resets targets and updates the lamps
	/// </summary>
	/// <param name="reset"></param>
	public override void TargetsCompleted(bool reset = true)
	{
		base.TargetsCompleted(reset);
		AdvanceCrossValue();
		UpdateLamps();
	}

	/// <summary>
	/// Updates the target lamps and cross stack
	/// </summary>
	public override void UpdateLamps()
	{
		base.UpdateLamps();

		//cross stack lamps, left side of table
		for (int i = 0; i < 4; i++)
		{
			pinGod.SetLampState("cross_" + i, player?.CrossStack[i] ?? 0);
		}
	}

    public override bool SetTargetComplete(int index)
    {
		pinGod.AddPoints(NightmareConstants.MED_SCORE);
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
		pinGod.PlaySfx("snd_ough");
		return base.SetTargetComplete(index);
    }
}
