using Godot;
using System;

/// <summary>
/// 1000 * each bonus times for player
/// Sends <see cref="PinGodGame.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class PbBonus : Control
{
	private Label label;
	private Label labelTotal;
	private VideoPlayerPinball video;
	private PinGodGame pinGod;
	private Timer timer;
	private int _bonusTimes;
	private int _bonusTimesLabel;
	private int _bonusTotal;
	private int _multiplier;
	const string BONUS_TEXT = @"PLAYER {PLAYER}
BONUS = {BONUS}
X {MULTIPLIER}
TOTAL";

	/// <summary>
	/// Awards the current player bonus and gets timer ref
	/// </summary>
	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as PinGodGame;
		if (pinGod.Player != null)
		{
			pinGod.Player.Points += pinGod.Player.Bonus;
		}
		timer = GetNode("Timer") as Timer;
		label = GetNode("CenterContainer/VBoxContainer/Label") as Label;
		labelTotal = GetNode("CenterContainer/VBoxContainer/Label2") as Label;
		video = GetNode<VideoPlayerPinball>("VideoPlayerPinball");
	}

	public override void _Ready()
	{
		if (!timer.IsStopped())
			timer.Stop();

		
		//start display - TEST
		//StartBonusDisplay();
	}

	public void StartBonusDisplay()
	{
		if (pinGod.IsTilted) return;
		_bonusTotal = 0;
		pinGod.PlayMusic("horsepin_i_see_you_move_it"); //todo paly position not working?
		var player = pinGod.Player as PlayboyPlayer;
		video.Play();
		if (player != null)
		{
			//todo: countdown lamps, countdown lamps again if a multiplier
			pinGod.LogInfo("player bonus_times: " + player.BonusTimes + " " + player.BonusTimes * 1000);

			if (player.SuperBonus)
				_bonusTotal += 20000;

			_bonusTimes = player.BonusTimes;			
			_multiplier = player.BonusMultiplier;
			label.Text = SetBonusText(pinGod.CurrentPlayerIndex+1, _multiplier, _bonusTimes);
			pinGod.LogDebug("bonus: set label text to:", label.Text);
		}
		else
		{
			//no player, scene running on it's own
			_bonusTimes = 25; _multiplier = 3;
			label.Text = SetBonusText(1, _multiplier, _bonusTimes);
		}

		_bonusTimesLabel = _bonusTimes;
		labelTotal.Text = 1000.ToScoreString();
		if(_bonusTimes > 10)
		{
			timer.Start(0.2f);
		}
		else
		{
			timer.Start(0.5f);
		}
		Visible = true;
	}

	/// <summary>
	/// Bonus has times out. Hide the display and send <see cref="PinGodGame.BonusEnded"/>
	/// </summary>
	private void _on_Timer_timeout()
	{
		if (_bonusTimesLabel <= 0)
		{			
			if(_multiplier <= 1)
			{
				pinGod.LogDebug("bonus: BonusEnded");
				timer.Stop();
				this.Visible = false;
				video.Stop();
				pinGod.AddPoints(_bonusTotal);
				pinGod.EmitSignal("BonusEnded");
			}
			else
			{
				_multiplier--;
				_bonusTimesLabel = _bonusTimes;
			}
		}
		else
		{
			_bonusTimesLabel--;
			_bonusTotal += 1000;
			labelTotal.Text = _bonusTotal.ToScoreString();
			GetParent().GetParent<Game>().UpdateBonusLamps();
		}
	}

	/// <summary>
	/// Generate a simple bonus amount string
	/// </summary>
	/// <returns></returns>
	private string SetBonusText(int player, int multiplier, int times)
	{
		return BONUS_TEXT.Replace("{PLAYER}", player.ToString())
			.Replace("{MULTIPLIER}", multiplier.ToString())
			.Replace("{BONUS}", times.ToString());
	}
}
