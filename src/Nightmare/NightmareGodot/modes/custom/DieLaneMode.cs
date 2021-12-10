using Godot;
using System.Linq;

/// <summary>
/// Die rollover lanes. Increases player multiplier when completed.
/// </summary>
public class DieLaneMode : PinballSwitchLanesNode
{
	/// <summary>
	/// Current player, populated when <see cref="OnBallStarted"/>
	/// </summary>
	private NightmarePlayer player;
    private Game game;

    public override void _EnterTree()
    {
        base._EnterTree();
		game = GetParent().GetParent() as Game;
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

	/// <summary>
	/// Advance multiplier when complete
	/// </summary>
	/// <returns></returns>
	public override bool CheckLanes()
	{
		var result = base.CheckLanes();
		if (result)
		{
			ResetLanesCompleted();
			AdvanceMultiplier();			
		}

		pinGod.PlaySfx("snd_inlane"); //todo: remix
		return result;
	}

	/// <summary>
	/// Add score when switch hit
	/// </summary>
	/// <param name="i"></param>
	/// <returns></returns>
	public override bool LaneSwitchActivated(int i)
	{
		pinGod.AddPoints(NightmareConstants.MED_SCORE/2);

		var complete = base.LaneSwitchActivated(i);
		if (complete)
		{

		}

		return complete;
	}

	/// <summary>
	/// Get the player and set multiplier to 1, reset lanes
	/// </summary>
	public void OnBallStarted()
	{
		player = game.GetPlayer();
		player.Multiplier = 1;
		player.Multipliers = new bool[5];
		this.ResetLanesCompleted();
		this.UpdateLamps();
	}

	/// <summary>
	/// Base updates the lanes and set the multiplier lamps here from player
	/// </summary>
	public override void UpdateLamps()
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
		//no process if all m-pliers are complete
		if(!player.Multipliers.Any(x => x))
		{			
			for (int i = 0; i < player.Multipliers.Length; i++)
			{
				if (!player.Multipliers[i])
				{
					player.Multipliers[i] = true;					
					if(i == 2) // x6
					{
						player.DoubleBonus = true;
						pinGod.LogInfo("die: double bonus activated");
					}
					else if (i == 4) // x10
					{
						player.ExtraBallLit = true;
						pinGod.LogInfo("die: double bonus activated");
					}

					if(game != null) game.PlayThenResumeMusic("mus_bonusmultiply", 2.3f);

					UpdateLamps();
					break;
				}
			}
		}
	}
}
