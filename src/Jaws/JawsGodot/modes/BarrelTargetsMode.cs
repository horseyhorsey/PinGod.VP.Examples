using Godot;
using System.Linq;

/// <summary>
/// Class to handle score from hitting the barrel targets. The more lights you have towards barrel multiball the more score. 
/// From FP:
/// '4 Barreltargets (just a bonus you get if it is hit):
/// 'BONUS INCREASES WITH BARREL LIGHTS ON!!!!!
/// TODO: Add extra bonus from these. Replace with Base Targets node
/// </summary>
public class BarrelTargetsMode : PinGodGameNode
{
	private Game game;
	private Timer timer;

	public override void _EnterTree()
	{        
		base._EnterTree(); // invoke this to get a PinGodGame node
		game = GetParent().GetParent() as Game;
		pinGod.Connect(nameof(PinGodGameBase.BallEnded), this, "OnBallEnded");
		timer = (GetNode("DoublePlayfieldTimer") as Timer);
	}
	public override void _Input(InputEvent @event)
	{
		if (!pinGod.GameInPlay || pinGod.IsTilted) return;

		if(pinGod.SwitchOn("barrel_target_0", @event))
		{
			pinGod.PlaySfx("barrel_target");
			TargetHit(0);
			UpdateLamps();
		}
		if (pinGod.SwitchOn("barrel_target_1", @event))
		{
			pinGod.PlaySfx("short_fx_03");
			TargetHit(1);
			UpdateLamps();
		}
		if (pinGod.SwitchOn("barrel_target_2", @event))
		{
			pinGod.PlaySfx("short_fx_03");
			TargetHit(2);
			UpdateLamps();
		}
		if (pinGod.SwitchOn("barrel_target_3", @event))
		{
			pinGod.PlaySfx("barrel_target");
			TargetHit(3);
			UpdateLamps();
		}

		if (!pinGod.IsMultiballRunning)
		{
			if (pinGod.SwitchOn("jaws_target_left", @event))
			{
				game.AddPoints(15000);
				game.currentPlayer.BonusBruce += game.DoublePlayfield ? 4950 * 2 : 4950;
				if (!game.IsMultiballScoringStarted)
				{
					pinGod.PlaySfx("barrel_target");
					pinGod.SolenoidPulse("flash_jaws");
				}
			}
			if (pinGod.SwitchOn("jaws_target_right", @event))
			{
				game.AddPoints(15000);
				game.currentPlayer.BonusBruce += game.DoublePlayfield ? 4950 * 2 : 4950;
				if (!game.IsMultiballScoringStarted)
				{
					pinGod.PlaySfx("barrel_target");
					pinGod.SolenoidPulse("flash_jaws");
				}
			}
		}		
	}

	private void _on_DoublePlayfieldTimer_timeout()
	{
		game.DoublePlayfield = false;
		game.UpdateProgress();
		game.currentPlayer.ResetBarrelTargets();
		UpdateLamps();
	}

	private void OnBallEnded(bool lastBall=false)
	{
		timer?.Stop();
		game.DoublePlayfield = false;
	}

	public void UpdateLamps()
	{
		for (int i = 0; i < game.currentPlayer.BarrelTargets.Length; i++)
		{
			if (game.currentPlayer.BarrelTargets[i])
				pinGod.SetLampState("barrel_target_" + i, 1);
			else
				pinGod.SetLampState("barrel_target_" + i, 0);
		}
	}

	bool CheckBarrelTargets() 
	{
		//double playfield already enabled
		if (game.DoublePlayfield)
			return false;

		if(!game.currentPlayer.BarrelTargets.Any(x=> !x))
		{
			game.DoublePlayfield = true;
			GD.Print("barrel targets completed. Double PF active");
			game.AddPoints(50000);			
			//todo: barrel bonus, bonus
			game.currentPlayer.BonusBarrel += game.DoublePlayfield ? 2750 * 2 : 2750;
			game.currentPlayer.BarrelTargetsComplete++;
			timer.Start();
			game.UpdateProgress();
			return true;
		}
		else
		{
			DisplayTargetInfo();
		}

		UpdateLamps();
		return false;
	}

	/// <summary>
	/// TODO
	/// </summary>
	void DisplayTargetInfo() { }

	/// <summary>
	/// TODO
	/// </summary>
	void HideInfoLayers() { }
	/// <summary>
	/// TODO
	/// </summary>
	void ResetBarrelTargets() { }
	/// <summary>
	/// TODO
	/// </summary>
	void SetUpTextLayers() { }
	bool TargetHit(int index) 
	{
		game.AddPoints(20000);
		game.currentPlayer.BonusBarrel += game.DoublePlayfield ? 2750 * 2 : 2750;
		if (!game.currentPlayer.BarrelTargets[index])
		{
			game.currentPlayer.BarrelTargets[index] = true;
			CheckBarrelTargets();
			return true;
		}

		return false;
	}
	void UpdateMultiplierLayers() { }
}



