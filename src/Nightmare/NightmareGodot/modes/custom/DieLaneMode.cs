using Godot;
using System.Linq;

/// <summary>
/// Die rollover lanes. Increases player multiplier when completed.
/// </summary>
public class DieLaneMode : PinGodGameMode
{
	/// <summary>
	/// Current player, populated when <see cref="OnBallStarted"/>
	/// </summary>
	private NightmarePlayer player;
    private Game game;
    private PinballLanesNode _lanes;

    public override void _EnterTree()
    {
        base._EnterTree();
		game = GetParent().GetParent() as Game;
		_lanes = GetNode<PinballLanesNode>(nameof(PinballLanesNode));
	}

    /// <summary>
    /// Base mode covers the rollover switches
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
	{
		if (!pinGod.IsBallStarted) return;
		if (pinGod.IsTilted) return;
		if (!pinGod.GameInPlay) return;        

		base._Input(@event);
	}

	void _on_PinballLanesNode_LaneCompleted(string swName, bool completed)
    {
		pinGod.AddPoints(NightmareConstants.MED_SCORE / 2);
		pinGod.AddBonus(NightmareConstants.SMALL_SCORE);
		if (completed)
		{
			pinGod.PlaySfx("snd_inlane"); //todo: remix and completed sound?
		}
		else
		{
			pinGod.PlaySfx("snd_inlane"); //todo: remix			
		}
	}

	void _on_PinballLanesNode_LanesCompleted()
    {
		AdvanceMultiplier();
    }

	protected override void OnBallStarted()
    {
        base.OnBallStarted();
		player = ((NightmarePinGodGame)pinGod).GetPlayer();
		player.Multiplier = 1;
		player.Multipliers = new bool[5];
		_lanes.ResetLanesCompleted();
		this.UpdateLamps();
	}

    /// <summary>
    /// Base updates the lanes and set the multiplier lamps here from player
    /// </summary>
    protected override void UpdateLamps()
	{
		base.UpdateLamps();
		
		if (player.Multipliers[0]) pinGod.SetLampState("x2", 1);
		else pinGod.SetLampState("x2", 0);
		if (player.Multipliers[1]) pinGod.SetLampState("x4", 1);
		else pinGod.SetLampState("x4", 0);
		if (player.Multipliers[2]) pinGod.SetLampState("x6", 1);
		else pinGod.SetLampState("x6", 0);
		if (player.Multipliers[3]) pinGod.SetLampState("x8", 1);
		else pinGod.SetLampState("x8", 0);
		if (player.Multipliers[4]) pinGod.SetLampState("x10", 1);
		else pinGod.SetLampState("x10", 0);
	}

	/// <summary>
	/// Sets double bonus, extra ball lit when advancing
	/// </summary>
	private void AdvanceMultiplier()
	{
		pinGod.LogInfo("advance multiplier");
		//no process if all m-pliers are complete
		if(player.Multipliers.Any(x => !x))
		{			
			for (int i = 0; i < player.Multipliers.Length; i++)
			{
				if (!player.Multipliers[i])
				{
					var msg = "MULTIPLIER\nINCREASED";
					player.Multipliers[i] = true;
					
					if(i == 0)
                    {
						player.Multiplier = 2;
					}
					else if (i == 0)
					{
						player.Multiplier = 4;
					}
					else if(i == 2) // x6
					{
						player.Multiplier = 6;
						player.DoubleBonus = true;
						pinGod.LogInfo("die: double bonus activated");
						msg += "\nDOUBLE BONUS\nACTIVATED";						
					}
					else if (i == 4) // x10
					{
						player.Multiplier = 10;
						player.ExtraBallLit = true;
						pinGod.LogInfo("die: extra ball activated");
						msg += "\nEXTRA BALL\nACTIVATED";
					}

					game.OnDisplayMessage(msg, 2.6f);
					pinGod.SolenoidOn("vpcoil", 6); //lampshow vp
					if (game != null) game.PlayThenResumeMusic("mus_bonusmultiply", 2.6f);

					UpdateLamps();
					break;
				}
			}
		}
	}
}
