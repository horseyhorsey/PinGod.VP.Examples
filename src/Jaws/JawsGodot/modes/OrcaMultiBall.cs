using Godot;
using System;
using System.Collections.Generic;

public class OrcaMultiBall : Control
{
	#region Properties / Fields
	bool _lastOrcaScene = false;
	private Timer ballLockedTimer;
	private Timer clearTimer;
	private Game game;
	private Label label;
	bool multiballReady = false;
	private JawsPinGodGame pinGod;
	public bool CanSkipScene { get; private set; }
	public bool IsMultiballRunning { get; set; }
	public VideoPlayerPinball VideoPlayer { get; private set; }
	public Dictionary<string, VideoStreamTheora> VideoStreams { get; private set; } 
	#endregion

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as JawsPinGodGame;
		VideoPlayer = GetNode("VideoPlayerPinball") as VideoPlayerPinball;
		label = GetNode("CenterContainer/Label") as Label;
		game = GetParent().GetParent() as Game;
		clearTimer = GetNode("HideTimer") as Timer;
		ballLockedTimer = GetNode("BallLockedTimer") as Timer;

		VideoStreams = new Dictionary<string, VideoStreamTheora>();
		VideoStreams.Add("quint_hoop_drives", new VideoStreamTheora() { File = "res://assets/videos/quint_hoop_drives.ogv" });
		VideoStreams.Add("orca_down_ramp", new VideoStreamTheora() { File = "res://assets/videos/orca_down_ramp.ogv" });
		VideoStreams.Add("orca_quint", new VideoStreamTheora() { File = "res://assets/videos/orca_quint.ogv" });
		VideoStreams.Add("orca_sail", new VideoStreamTheora() { File = "res://assets/videos/orca_sail.ogv" });
		VideoStreams.Add("orca_mball_start", new VideoStreamTheora() { File = "res://assets/videos/orca_mball_start.ogv" });		
	}
	public override void _Input(InputEvent @event)
	{
		if (pinGod.IsTilted || !pinGod.GameInPlay) return;

		if (pinGod.SwitchOn("orca_magnet", @event))
		{
			OrcaMagnetHit();
		}

		if (!game.IsMultiballScoringStarted && CanSkipScene && !game.IsHurryUpRunning())
		{
			if (pinGod.SwitchOn("orca_magnet"))
			{
				if (pinGod.SwitchOn("flipper_l", @event) && pinGod.SwitchOn("flipper_r"))
				{
					CanSkipScene = false;
					ballLockedTimer.Stop();
					game.AddPoints(5000);
					game.currentPlayer.BonusSkipper += game.DoublePlayfield ? Game.BONUS_SKIPPER_VALUE * 2 : Game.BONUS_SKIPPER_VALUE;					
					BallLocked();
				}
			}
		}		

		//TODO skip scenes
		//game.currentPlayer.BonusSkipper += game.DoublePlayfield ? 1000 * 2 : 1000;
		if (pinGod.SwitchOn("orca_target", @event))
		{
			OrcaTargetHit();
		}
	}
	public void DisableOrcaLamps()
	{
		pinGod.SetLampState("orca_0", 0); pinGod.SetLampState("orca_1", 0);
		pinGod.SetLampState("orca_2", 0); pinGod.SetLampState("orca_3", 0);
	}
	/// <summary>
	/// Updates the Orca lamps and multi-ball bulbs
	/// </summary>
	public void UpdateLamps()
	{
		if (this.IsMultiballRunning)
		{
			pinGod.SetLampState("orca_multiball", 2);
			pinGod.SetLampState("orca_jackpot_bulb", 1);
		}
		else
		{
			pinGod.SetLampState("orca_jackpot_bulb", 0);
			if (game.currentPlayer.OrcaMballComplete) pinGod.SetLampState("orca_multiball", 1);
			else pinGod.SetLampState("orca_multiball", 0);
		}

		if (pinGod.GetJawsPlayer().OrcaLocksActive)
		{
			var cnt = pinGod.GetJawsPlayer().OrcaCount;
			pinGod.SetLampState("orca_target", 1);
			switch (cnt)
			{
				case 0:
					pinGod.SetLampState("orca_0", 2);
					break;
				case 1:
					pinGod.SetLampState("orca_0", 1);
					pinGod.SetLampState("orca_1", 2);
					break;
				case 2:
					pinGod.SetLampState("orca_0", 1);
					pinGod.SetLampState("orca_1", 1);
					pinGod.SetLampState("orca_2", 2);
					break;
				case 3:
					pinGod.SetLampState("orca_0", 1);
					pinGod.SetLampState("orca_1", 1);
					pinGod.SetLampState("orca_2", 1);
					pinGod.SetLampState("orca_3", 2);
					break;
				case 4:
					pinGod.SetLampState("orca_0", 1);
					pinGod.SetLampState("orca_1", 1);
					pinGod.SetLampState("orca_2", 1);
					pinGod.SetLampState("orca_3", 1);
					break;
				default:
					break;
			}
		}
		else
		{
			pinGod.SetLampState("orca_target", 2);
		}
	}
	private void _on_BallLockedTimer_timeout()
	{
		BallLocked();
	}
	private void _on_HideTimer_timeout()
	{
		this.Visible = false;
	}
	void BallLocked()
	{
		if (!pinGod.IsMultiballRunning)
		{
			if (!multiballReady)
			{
				CallDeferred("PlayScene", "orca_down_ramp");
				clearTimer.Start(2f);
			}
			else
			{
				multiballReady = false;
				IsMultiballRunning = true;
				game.currentPlayer.ResetOrca();
				GD.Print("orca: starting m-ball");
				pinGod.StartMultiBall(2, 20, 2); //two ball multi-ball
				game.AddMultiScoringMode();
				clearTimer.Start(5f);
				pinGod.PlayMusic("orca_mball");
			}
		}

		game.ReleaseOrcaMagnet();
	}
	private void EndMultiball()
	{
		if (IsMultiballRunning)
		{
			IsMultiballRunning = false;			
			DisableOrcaLamps();
			UpdateLamps();
			GD.Print("orca: _OnMultiBallEnded");
		}
	}
	private void OrcaMagnetHit()
	{
		GD.Print("orca magnet: hit");
		if (pinGod.IsMultiballRunning)
		{
			
		}
		else if (pinGod.GetJawsPlayer().BarrelsOn) 
		{
			GD.Print("orca magnet: barrels up");
			game.PlayBarrelReminderScene();
		}
		else if (game.currentPlayer.OrcaLocksActive)
		{
			GD.Print("orca magnet: activated");
			game.AddPoints(20000);
			game.ReleaseOrcaMagnet(false);
			game.currentPlayer.Bonus += Game.BONUS_VALUE;
			this.label.Text = "RAMP\r\nACTIVE";			

			if (game.currentPlayer.OrcaCount == 3) //multi-ball start
			{
				StartMultiball();
			}
			else
			{				
				game.currentPlayer.OrcaCount++;
				this.label.Text = $"\r\n\r\n\n\rORCA LOCK{game.currentPlayer.OrcaCount}";
				this.Visible = true;
				clearTimer.Stop();
				ballLockedTimer.Start(4f);
				//cycle the video that plays
				if (!_lastOrcaScene)
					CallDeferred("PlayScene", "orca_quint");
				else
					CallDeferred("PlayScene", "orca_sail");
				_lastOrcaScene = !_lastOrcaScene;
				CanSkipScene = true;
			}
		}
		else
		{
			this.label.Text = "TARGET\r\nACTIVATES ORCA";
			this.Visible = true;
			clearTimer.Stop();clearTimer.Start(2);
		}
	}

	private void StartMultiball()
	{
		pinGod.AudioManager.StopMusic();
		IsMultiballRunning = true;
		game.currentPlayer.OrcaMballCompleteCount++;
		//todo: blink all show
		this.Visible = true;
		game.SetGiState(LightState.Blink);
		clearTimer.Stop();
		multiballReady = true;
		ballLockedTimer.Start(3.6f);
		game.currentPlayer.OrcaCount++;
		this.label.Text = $"\n\r\n\rORCA\r\nMULTIBALL";
		CallDeferred("PlayScene", "orca_mball_start");
		game.UpdateProgress();
	}

	private void OrcaTargetHit()
	{
		game.AddPoints(10000);
		game.currentPlayer.Bonus += Game.BONUS_VALUE;
		pinGod.PlaySfx("barrel_target");
		//todo: flashOrcaM
		if (!pinGod.IsMultiballRunning && !pinGod.GetJawsPlayer().OrcaLocksActive)
		{
			pinGod.GetJawsPlayer().OrcaLocksActive = true;
			label.Text = "\r\nORCA RAMP\r\nOPEN";
			this.Visible = true;
			clearTimer.Start(2.6f);
			UpdateLamps();
			CallDeferred("PlayScene", "quint_hoop_drives");			
		}
	}
	private void PlayScene(string name)
	{
		GD.Print("playing scene: ", name);
		VideoPlayer.Stream = VideoStreams[name];
		VideoPlayer.Play();
		VideoPlayer.Visible = true;
	}
	private void ReleaseOrcaMagnetMultiball()
	{
		game.ReleaseOrcaMagnet(true);
		UpdateLamps();
	}
}
