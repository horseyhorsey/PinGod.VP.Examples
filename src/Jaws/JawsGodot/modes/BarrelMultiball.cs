using Godot;
using System.Collections.Generic;

/// <summary>
/// Barrel shots (videos)?
/// </summary>
public class BarrelMultiball : Control
{
	#region Fields
	private BallStackPinball _ballStackPinball;
	private Timer _clearLayersTimer;
	private BlinkingLabel _label;
	private Game game;
	private JawsPinGodGame pinGod;
	#endregion

	#region Properties    
	public bool CanSkipScene { get; private set; }
	public bool IsMulitballRunning { get; set; }
	public bool IsTargetsUp { get; set; }
	public VideoPlayerPinball VideoPlayer { get; private set; }
	public Dictionary<string, VideoStreamTheora> VideoStreams { get; private set; } 
	#endregion

	public override void _EnterTree()
	{
		pinGod = GetNode("/root/PinGodGame") as JawsPinGodGame;

		game = GetParent().GetParent() as Game;

		//get the ball stack to use input processing for the stack switches
		_ballStackPinball = GetNode("BallStackPinball") as BallStackPinball;

		VideoPlayer = GetNode("VideoPlayerPinball") as VideoPlayerPinball;

		_label = GetNode("CenterContainer/BlinkingLabel") as BlinkingLabel;
		_clearLayersTimer = GetNode("ClearLayersTimer") as Timer;

		VideoStreams = new Dictionary<string, VideoStreamTheora>();
		VideoStreams.Add("barrel_hook_me_up", new VideoStreamTheora() { File = "res://assets/videos/barrels/barrel_hook_me_up.ogv" });
		VideoStreams.Add("barrel_round_complete", new VideoStreamTheora() { File = "res://assets/videos/barrels/barrel_round_complete.ogv" });
		VideoStreams.Add("barrel_shot_1", new VideoStreamTheora() { File = "res://assets/videos/barrels/barrel_shot_1.ogv" });
		VideoStreams.Add("barrel_shot_2", new VideoStreamTheora() { File = "res://assets/videos/barrels/barrel_shot_2.ogv" });
		VideoStreams.Add("barrel_start", new VideoStreamTheora() { File = "res://assets/videos/barrels/barrel_start.ogv" });
		VideoStreams.Add("barrel_tie_it_on", new VideoStreamTheora() { File = "res://assets/videos/barrels/barrel_tie_it_on.ogv" });
		VideoStreams.Add("brody_barrel_1", new VideoStreamTheora() { File = "res://assets/videos/barrels/brody_barrel_1.ogv" });
		VideoStreams.Add("brody_barrel_2", new VideoStreamTheora() { File = "res://assets/videos/barrels/brody_barrel_2.ogv" });
		VideoStreams.Add("quint_3_barrels", new VideoStreamTheora() { File = "res://assets/videos/barrels/quint_3_barrels.ogv" });
	}
	public override void _Input(InputEvent @event)
	{
		if (pinGod.IsTilted || !pinGod.GameInPlay) return;

		if(game.currentPlayer != null)
		{
			//player can skip the scene by ending the timer and checking multi-ball. both flippers with the kicker enabled
			if (!game.IsMultiballScoringStarted && CanSkipScene && !game.currentPlayer.BarrelsOn)
			{
				if (pinGod.SwitchOn("barrel_kicker"))
				{
					if (pinGod.SwitchOn("flipper_l", @event) && pinGod.SwitchOn("flipper_r"))
					{
						BarrelSceneSkip();
					}
				}
			}

			if (game.currentPlayer.BarrelsOn)
			{
				if (pinGod.SwitchOn("drop_0", @event))
				{
					var complete = DropTargetCheck(0);
					if (complete)
					{
						pinGod.SolenoidOn("drop_0", 0);
						var dropsComplete = IsDropsComplete();
						if (dropsComplete)
							OnBarrelDropsCompleted();
						UpdateLamps();
						DropTargetHitAward(dropsComplete);
					}
				}
				if (pinGod.SwitchOn("drop_1", @event))
				{
					var complete = DropTargetCheck(1);
					if (complete)
					{
						pinGod.SolenoidOn("drop_1", 0);
						var dropsComplete = IsDropsComplete();
						if (dropsComplete)
							OnBarrelDropsCompleted();
						UpdateLamps();
						DropTargetHitAward(dropsComplete);
					}
				}
				if (pinGod.SwitchOn("drop_2", @event))
				{
					var complete = DropTargetCheck(2);
					if (complete)
					{
						pinGod.SolenoidOn("drop_2", 0);
						var dropsComplete = IsDropsComplete();
						if (dropsComplete)
							OnBarrelDropsCompleted();
						DropTargetHitAward(dropsComplete);
						UpdateLamps();
					}
				}
				if (pinGod.SwitchOn("drop_3", @event))
				{
					var complete = DropTargetCheck(3);
					if (complete)
					{
						pinGod.SolenoidOn("drop_3", 0);
						var dropsComplete = IsDropsComplete();
						if (dropsComplete)
							OnBarrelDropsCompleted();
						DropTargetHitAward(dropsComplete);
						UpdateLamps();
					}
				}
				if (pinGod.SwitchOn("drop_4", @event))
				{
					var complete = DropTargetCheck(4);
					if (complete)
					{
						pinGod.SolenoidOn("drop_4", 0);
						var dropsComplete = IsDropsComplete();
						if (dropsComplete)
							OnBarrelDropsCompleted();
						DropTargetHitAward(dropsComplete);
						UpdateLamps();
					}
				}
				if (pinGod.SwitchOn("drop_5", @event))
				{
					var complete = DropTargetCheck(5);
					if (complete)
					{
						pinGod.SolenoidOn("drop_5", 0);
						var dropsComplete = IsDropsComplete();
						if (dropsComplete)
							OnBarrelDropsCompleted();
						DropTargetHitAward(dropsComplete);
						UpdateLamps();
					}
				}
			}
		}		
	}
	public void PlayBarrelReminderScene(bool showScore = true)
	{
		Visible = true;
		ClearLayerTimer(1.8f);
		if (!showScore) _label.Visible = true;
		else _label.Visible = false;
		CallDeferred("PlayScene", "barrel_hook_me_up");
	}
	public void UpdateLamps()
	{
		if (IsMulitballRunning)
		{
			pinGod.SetLampState("barrel_multiball", 2);
		}
		else if (game.currentPlayer.BarrelMballCompleteCount > 0)
		{
			pinGod.SetLampState("barrel_multiball", 1);
		}

		var cnt = pinGod.GetJawsPlayer().BarrelStage;
		switch (cnt)
		{
			case 0:
				pinGod.SetLampState("barrel_0", 2);
				break;
			case 1:
				pinGod.SetLampState("barrel_0", 1);
				pinGod.SetLampState("barrel_1", 2);
				break;
			case 2:
				pinGod.SetLampState("barrel_0", 1);
				pinGod.SetLampState("barrel_1", 1);
				pinGod.SetLampState("barrel_2", 2);
				break;
			case 3:
				pinGod.SetLampState("barrel_0", 1);
				pinGod.SetLampState("barrel_1", 1);
				pinGod.SetLampState("barrel_2", 1);
				pinGod.SetLampState("barrel_3", 2);
				break;
			case 4:
				pinGod.SetLampState("barrel_0", 1);
				pinGod.SetLampState("barrel_1", 1);
				pinGod.SetLampState("barrel_2", 1);
				pinGod.SetLampState("barrel_3", 1);
				break;
			default:
				break;
		}

		if (game.currentPlayer.BarrelsOn)
		{
			for (int i = 0; i < game.currentPlayer.DropTargets.Length; i++)
			{
				var b = game.currentPlayer.DropTargets[i];
				if (!b) pinGod.SetLampState($"drop_{i}", 2);
				else pinGod.SetLampState($"drop_{i}", 0);
			}
		}
	}
	private void BarrelSceneSkip()
	{
		_ballStackPinball.Stop(); //stop the kicker timer
		CanSkipScene = false;
		game.AddPoints(5000);
		game.currentPlayer.BonusSkipper += game.DoublePlayfield ? Game.BONUS_SKIPPER_VALUE * 2 : Game.BONUS_SKIPPER_VALUE;
		CheckForMultiball();
	}
	#region Support methods

	private void _on_ClearLayersTimer_timeout()
	{
		Visible = false;
	}

	private void _on_BallStackPinball_SwitchActive()
	{
		pinGod.LogDebug("barrel kicker active");
		BarrelKickerHit();
	}

	private void _on_BallStackPinball_SwitchInActive()
	{
		pinGod.LogDebug("barrel kicker inactive");
	}

	private void _on_BallStackPinball_timeout()
	{
		pinGod.LogDebug("ball stack timer ended");
		Visible = false;
		if (!pinGod.IsMultiballRunning)
		{
			var result = CheckForMultiball();
			pinGod.LogInfo("barrel_kicker: exit - m-ball on? ", result);
		}
		else
		{
			KickBall();
		}

		pinGod.SolenoidOn("flash_barrel", 0);
		if (!pinGod.IsMultiballRunning)
			game.UpdateLamps();
	}
	private void BarrelHitChecker()
	{
		if (!game.currentPlayer.BarrelsOn)
		{
			pinGod.LogInfo("barrel_kicker: incrementing barrel stage to:", game.currentPlayer.BarrelStage + 1);
			game.currentPlayer.BarrelStage++;
			pinGod.PlayMusic("barrels_loop");

			if (game.currentPlayer.BarrelStage > 0)
			{
				CanSkipScene = true;
				switch (game.currentPlayer.BarrelStage)
				{
					case 1:
						_ballStackPinball.Start(3.3f); //kick the ball after 3.3 secs
						_label.Text = "\n\r\n\rBARREL MULTIBALL\r\nROUND 1";
						CallDeferred("PlayScene", "brody_barrel_1");
						break;

					case 2:
						_ballStackPinball.Start(6.2f); //kick the ball after 6.2 secs unless skipped
						_label.Text = "\n\r\n\rBARREL MULTIBALL\r\nROUND 2";
						CallDeferred("PlayScene", "brody_barrel_2");
						break;

					case 3:
						_ballStackPinball.Start(3.2f);
						_label.Text = "\n\r\n\rBARREL MULTIBALL\r\nROUND 3";
						CallDeferred("PlayScene", "quint_3_barrels");
						break;
					default:
						_label.Text = "\n\r\n\rBARREL MULTIBALL\r\n";
						CanSkipScene = false;
						CheckForMultiball();
						break;
				}

				_label.Visible = true;
			}
		}

		if (game.IsHurryUpRunning())
			game.StopHurryUp();
	}
	/// <summary>
	/// Main barrel scoop kicker
	/// </summary>
	private void BarrelKickerHit()
	{
		game.EnableKickback(true); ///todo@disable kickback when done
		game.AddPoints(25000);
		game.currentPlayer.BonusBarrel += game.DoublePlayfield ? Game.BONUS_BARREL_VALUE * 2 : Game.BONUS_BARREL_VALUE;

		if (pinGod.IsMultiballRunning)
		{
			pinGod.SolenoidOn("flash_barrel", 1);
			_ballStackPinball.Start(1);
		}
		else if (game.currentPlayer.BarrelsOn) //exit scoop because in the target mode
		{
			pinGod.LogInfo("barrel_kicker: targets already up, exiting");
			pinGod.SolenoidOn("flash_barrel", 1);
			_ballStackPinball.Start(1f);
		}
		else if (!game.IsMultiballScoringStarted)
		{
			BarrelHitChecker();
		}
		else
		{
			pinGod.SolenoidOn("flash_barrel", 1);
			_ballStackPinball.Start(1);
		}
	}
	/// <summary>
	/// Scene to show score
	/// </summary>
	/// <param name="points"></param>
	private void BarrelTargetHit(long points)
	{
		ClearLayerTimer(1.8f);
		_label.Text = points.ToScoreString();
		CallDeferred("PlayScene", "barrel_shot_1");
	}
	/// <summary>
	/// Sets <see cref="game.currentPlayer.BarrelTargetsUp"/> or starts a multiball.
	/// </summary>
	private bool CheckForMultiball()
	{
		CanSkipScene = false;
		var stage = game.currentPlayer.BarrelStage;
		switch (stage)
		{
			case 1:
			case 2:
				game.currentPlayer.BarrelsOn = true;
				pinGod.LogInfo("barrel drop targets enabled");
				ResetDrops();
				KickBall();
				PlayBarrelReminderScene();
				return false;
			case 3:
				game.currentPlayer.BarrelsOn = true;
				ResetDrops();
				KickBall();
				_label.Text = "HIT TARGETS\n\rTO ADVANCE";
				ClearLayerTimer(1.8f);
				CallDeferred("PlayScene", "barrel_tie_it_on");
				return false;

			case 4:
				StartMultiball();
				game.AddMultiScoringMode();
				return true;

			default:
				return false;
		}
	}
	/// <summary>
	/// Stops then starts the clear layers timer
	/// </summary>
	/// <param name="time"></param>
	void ClearLayerTimer(float time)
	{
		_clearLayersTimer.Stop();
		_clearLayersTimer.Start(time);
	}
	/// <summary>
	/// Disables the barrel drops targets
	/// </summary>
	private void DisableDrops()
	{
		game.DisableBarrelTargets();
	}
	private bool DropTargetCheck(byte index)
	{
		var p = pinGod.GetJawsPlayer();
		var dtargets = p.DropTargets;
		if (!dtargets[index])
		{
			pinGod.LogDebug("barrel: drop_t" + index, " hit");
			dtargets[index] = true;
			return true;
		}

		return false;
	}
	/// <summary>
	/// 100K per barrel *  barrel bonus targets completed * barrels completed
	/// </summary>
	private void DropTargetHitAward(bool skipAwardScene = false)
	{
		var barrelMballCompleted = game.currentPlayer.BarrelMballCompleteCount;
		//todo: barrel_target_multiplier = sum(self.game.barrel_targets_mode.BarrelTargets) + 1
		var bTargetMplier = 2;
		var score = 100000 * bTargetMplier;
		var awardedScore = 0;
		game.currentPlayer.BonusBarrel += game.DoublePlayfield ? Game.BONUS_BARREL_VALUE * 2 : Game.BONUS_BARREL_VALUE;
		if (barrelMballCompleted >= 1)
		{
			awardedScore = score * (barrelMballCompleted + 1);
			awardedScore = game.AddPoints(awardedScore);
		}
		else
		{
			awardedScore = game.AddPoints(score); ;
		}

		if (!skipAwardScene)
		{
			CallDeferred("BarrelTargetHit", awardedScore);
		}
	}
	private void EndMultiball()
	{
		game.currentPlayer.BarrelsOn = false;		
		game.currentPlayer.BarrelStage = 0;
		game.currentPlayer.BarrelMballCurrentScore = 0;
		IsMulitballRunning = false;
		pinGod.LogInfo("barrel: _OnMultiBallEnded");
	}
	private bool IsDropsComplete()
	{
		bool completed = true;
		var p = pinGod.GetJawsPlayer();
		var dtargets = p.DropTargets;
		for (int i = 0; i < dtargets.Length; i++)
		{
			if (!dtargets[i])
			{
				completed = false;
				break;
			}
		}

		return completed;
	}
	private void KickBall() { pinGod.SolenoidPulse("barrel_kicker"); }
	private void OnBarrelDropsCompleted()
	{
		pinGod.LogInfo("barrel drops completed");
		game.ResetBarrelDropsRound(); //reset barrel vars
		ClearLayerTimer(2.5f);
		_label.Text = "BARRELS\r\nHOOKED";
		_label.Visible = true;
		CallDeferred("PlayScene", "barrel_round_complete");
		//pinGod.PlaySfx("quint_target_fx");

		game.PlayMusicForMode();		
	}
	private void PlayScene(string name)
	{
		Visible = true;
		VideoPlayer.Stream = VideoStreams[name];
		VideoPlayer.Play();
		VideoPlayer.Visible = true;
	}
	/// <summary>
	/// Resets the players drop targets. Pops up a number of targets depending how many stages and m-ball is complete
	/// </summary>
	private void ResetDrops()
	{
		game.currentPlayer.ResetDropTargets();

		var bCompleteCnt = game.currentPlayer.BarrelMballCompleteCount;
		var bStage = game.currentPlayer.BarrelStage;
		//m-ball was completed more than once, show all targets
		if (bCompleteCnt > 1)
		{
			StartBarrelTargets(6);
		}
		else
		{
			if (bCompleteCnt == 0)
			{
				StartBarrelTargets(bStage); //targets the same as the stage number
			}
			else
			{
				StartBarrelTargets(bStage + 3); //stage 1 == 4, 2==5, 3==6
			}
		}
	}
	private void StartBarrelTargets(int numOfTargets)
	{
		pinGod.LogDebug("bMball: setting up targets");
		DisableDrops(); //disable targets
		pinGod.EnableJawsToy(true); //turn on the shark (closing mouth)
		if (numOfTargets == 6)
		{
			game.currentPlayer.ResetDropTargets();
			for (int i = 0; i < game.currentPlayer.DropTargets.Length; i++)
			{
				pinGod.SolenoidOn("drop_" + i, 1);
				game.currentPlayer.DropTargets[i] = false;
			}
		}
		else
		{
			pinGod.LogDebug("bMball: enabling targets : ", numOfTargets);
			int[] arr = new int[] { 0, 1, 2, 3, 4, 5 };
			arr = PinGodExtensions.ShuffleArray(arr, numOfTargets);
			for (int i = 0; i < arr.Length; i++)
			{
				var index = arr[i];
				game.currentPlayer.DropTargets[index] = false;
				pinGod.SolenoidOn("drop_" + index, 1);
				pinGod.LogInfo("drop_" + index);
			}
		}		
	}
	private void StartMultiball()
	{
		IsMulitballRunning = true;
		pinGod.StartMultiBall(3, 20);
		KickBall();
		_label.Text = "BARREL\r\nMULTI-BALL";
		pinGod.PlayMusic("barrel_start");
		ClearLayerTimer(3f);
		CallDeferred("PlayScene", "barrel_start");
		game.currentPlayer.BarrelMballCompleteCount++;
		game.UpdateProgress();
	} 
	#endregion
}


