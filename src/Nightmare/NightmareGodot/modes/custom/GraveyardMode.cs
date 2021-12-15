/// <summary>
/// RIP targets advance graveyard letters. Opens spin scoop
/// </summary>
public class GraveyardMode : PinballTargetsControl
{
    private Game game;
    private NightmarePlayer player;
    public override void _EnterTree()
	{
		base._EnterTree();
		game = GetParent().GetParent() as Game;
	}
	public override bool CheckTargetsCompleted(int index)
	{
		var result = base.CheckTargetsCompleted(index);
		if (!result)
			UpdateLamps();
		return result;
	}

    /// <summary>
    /// Gets player from game. Resets targets RIP
    /// </summary>
    public void OnBallStarted()
    {
        player = ((NightmarePinGodGame)pinGod).GetPlayer();
        ResetTargets();
    }

    /// <summary>
    /// Targets complete lights mystery spin and advances graveyard
    /// </summary>
    /// <param name="reset"></param>
    public override void TargetsCompleted(bool reset = true)
	{
		base.TargetsCompleted(reset);

		player.MysterySpinLit = true;

		AdvanceGraveyard();		
		if (game != null)
		{
			game.PlayThenResumeMusic("mus_graveyardletter", 2f);
			pinGod.UpdateLamps(game.GetTree());
		}
		pinGod.LogInfo("grave: mystery lit and graveyard advanced");
	}

    public override bool SetTargetComplete(int index)
	{
		pinGod.PlaySfx("snd_ough");
		pinGod.AddPoints(NightmareConstants.MED_SCORE);
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
		return base.SetTargetComplete(index);
    }

    /// <summary>
    /// Updates targets lamps and graveyard lamps center play-field
    /// </summary>
    public override void UpdateLamps()
	{
		base.UpdateLamps();

		for (int i = 0; i < 9; i++)
		{
			var lamp = "grave_" + i;
			if (i + 1 > player.GraveYardValue) pinGod.SetLampState(lamp, 0);
			else pinGod.SetLampState(lamp, 1);
		}
	}

	/// <summary>
	/// Advances the players graveyard value, resets to zero when 9
	/// </summary>
	void AdvanceGraveyard()
	{
		player.GraveYardValue++;		

		if (player.GraveYardValue == 9)
		{
			//todo: something awarded at 9?
			player.GraveYardValue = 0;
			pinGod.LogDebug("graveyard value reset to 0");
		}
        else
        {
			game.OnDisplayMessage($"GRAVEYARD ADVANCED\nLEVEL {player.GraveYardValue}");
			pinGod.LogDebug("grave: value", player.GraveYardValue);
			pinGod.SolenoidOn("vpcoil", 7); //lampshow vp
		}		
	}
}
