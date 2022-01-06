using Godot;

public class RightTargetsMode : PinGodGameMode
{
    private PinballTargetsBank _targets;
    private Game game;
    private NightmarePlayer player;

	public override void _EnterTree()
	{
		base._EnterTree();
		//get game to resume music
		game = GetParent().GetParent() as Game;

		_targets = GetNode<PinballTargetsBank>(nameof(PinballTargetsBank));
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
			pinGod.LogInfo("advancing cross value to ", player.CrossValue);
			if (player.CrossValue < 6)
			{
				var currentCross = player.CrossStack[player.CrossValue - 1];
				if (currentCross == 0)
				{
					player.CrossStack[player.CrossValue - 1] = 2;
					pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
				}

				//get game stop music and resume
				if (game != null)
				{
					game.PlayThenResumeMusic("mus_advancecrossstack", 2.0f);

					pinGod.SolenoidOn("vpcoil", 5); // lampshow vp
					//run UpdateLamps in groups marked as Mode
					pinGod.UpdateLamps(game?.GetTree());
				}
				else pinGod.LogWarning("no game object found to use timer and resume music");

				pinGod.LogInfo("Targets: advance cross value", player.CrossValue);				
			}
		}
		else pinGod.LogWarning("No player found");

		game.OnDisplayMessage($"CROSS ADVANCED\n{player?.CrossValue ?? 1}", 2f);
	}

	/// <summary>
	/// reset cross value and targets
	/// </summary>
	protected override void OnBallStarted()
	{
		player = ((NightmarePinGodGame)pinGod).GetPlayer();
		_targets.ResetTargets();
		UpdateLamps();
	}

	/// <summary>
	/// Updates the target lamps and cross stack
	/// </summary>
	protected override void UpdateLamps()
	{
		base.UpdateLamps();

		//cross stack lamps, left side of table
		for (int i = 0; i < 4; i++)
		{
			pinGod.SetLampState("cross_" + i, player?.CrossStack[i] ?? 0);
		}
	}

	void _on_PinballTargetsBank_OnTargetActivated(string swName, bool complete)
    {
		pinGod.AddPoints(NightmareConstants.MED_SCORE);
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
		pinGod.PlaySfx("snd_ough");

		if (!complete)
			UpdateLamps();
	}

	void _on_PinballTargetsBank_OnTargetsCompleted()
    {
		_targets.TargetsCompleted(true);
		AdvanceCrossValue();
		UpdateLamps();
	}
}
