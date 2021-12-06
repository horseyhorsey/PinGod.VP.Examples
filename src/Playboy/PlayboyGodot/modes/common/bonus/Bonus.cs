using Godot;
using System;

/// <summary>
/// 1000 * each bonus times for player
/// Sends <see cref="PinGodGame.BonusEnded"/> signal on <see cref="_display_for_seconds"/> timeout
/// </summary>
public class Bonus : Control
{
	[Export] int _display_for_seconds = 5;

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
		var player = pinGod.Player as PlayboyPlayer;
		_bonusTimes = 25; _multiplier = 3;
		video.Play();
		if (player != null)
		{
			//todo: countdown lamps, countdown lamps again if a multiplier
			pinGod.LogInfo("player bonus_times: " + player.BonusTimes + " " + player.BonusTimes * 1000);

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
		timer.Start(0.5f);
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
			UpdateBonusLamps(_bonusTimesLabel);
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

	private void UpdateBonusLamps(int bonusCount)
	{
		var cnt = bonusCount;
		//disable all bonus lamps
		for (int i = 1; i < 11; i++) pinGod.SetLampState("bonus_" + i, 0);

		if (cnt > 0)
		{
			//set a single number lamp
			var lmpCnt = cnt > 10 ? Convert.ToInt32(cnt.ToString().Substring(1)) : cnt;
			if(lmpCnt > 0) pinGod.SetLampState("bonus_" + lmpCnt, 1);

			if (cnt % 10 == 0) pinGod.SetLampState("bonus_9", 1);

			if (cnt == 20)
			{
				pinGod.SetLampState("bonus_9", 0);
				pinGod.SetLampState("bonus_10", 1);
				pinGod.SetLampState("bonus_11", 1);
			}
			else if (cnt > 20) pinGod.SetLampState("bonus_11", 1);
		}
		else
		{
			//pinGod.SetLampState("bonus_" + cnt, (byte)LightState.On);
		}
	}
}
