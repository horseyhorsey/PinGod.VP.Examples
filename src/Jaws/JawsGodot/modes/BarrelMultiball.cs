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

        GetResources();
    }

    public override void _Input(InputEvent @event)
    {
        if (pinGod.IsTilted || !pinGod.GameInPlay) return;

        if (game.currentPlayer != null)
        {
            //player can skip the scene by ending the timer and checking multi-ball. both flippers with the kicker enabled
            if (!game.IsMultiballScoringStarted && CanSkipScene && !game.currentPlayer.BarrelsOn)
            {
                if (pinGod.SwitchOn("barrel_kicker"))
                {
                    if (pinGod.SwitchOn("flipper_r") && pinGod.SwitchOn("flipper_l", @event))
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
            case 1:
                pinGod.SetLampState("barrel_0", 2);
                break;
            case 2:
                pinGod.SetLampState("barrel_0", 1);
                pinGod.SetLampState("barrel_1", 2);
                break;
            case 3:
                pinGod.SetLampState("barrel_0", 1);
                pinGod.SetLampState("barrel_1", 1);
                pinGod.SetLampState("barrel_2", 2);
                break;
            case 4:
                pinGod.SetLampState("barrel_0", 1);
                pinGod.SetLampState("barrel_1", 1);
                pinGod.SetLampState("barrel_2", 1);
                pinGod.SetLampState("barrel_3", 2);
                break;
            case 5:
                pinGod.SetLampState("barrel_0", 1);
                pinGod.SetLampState("barrel_1", 1);
                pinGod.SetLampState("barrel_2", 1);
                pinGod.SetLampState("barrel_3", 1);
                break;
            default:
                break;
        }

        //drop target lamps
        if (game.currentPlayer.BarrelsOn)
        {
            for (int i = 0; i < game.currentPlayer.DropTargets.Length; i++)
            {
                var b = game.currentPlayer.DropTargets[i];
                if (!b) pinGod.SetLampState($"drop_{i}", 2);
                else pinGod.SetLampState($"drop_{i}", 0);
            }
        }
        else
        {
            for (int i = 0; i < game.currentPlayer.DropTargets.Length; i++)
            {
                pinGod.SetLampState("drop_" + i, 0);
            }
        }
    }

    private void _on_BallStackPinball_SwitchActive()
    {
        pinGod.LogDebug("barrel_kicker active");
        CallDeferred(nameof(BarrelKickerHit));
    }

    private void _on_BallStackPinball_SwitchInActive()
    {
        pinGod.LogDebug("barrel kicker inactive");
    }

    private void _on_BallStackPinball_timeout()
    {
        pinGod.LogDebug("ball stack timer ended");
        Visible = false;
        if (!pinGod.IsMultiballRunning && !game.currentPlayer.BarrelsOn)
        {
            var result = CheckForMultiball();
            pinGod.LogInfo("barrel_kicker: exit - m-ball on? ", result);
            if (result)
                KickBall();
        }
        else
        {
            KickBall();
        }

        pinGod.SolenoidOn("flash_barrel", 0);
        if (!pinGod.IsMultiballRunning)
            game.UpdateLamps();
    }

    private void _on_ClearLayersTimer_timeout()
    {
        Visible = false;
    }

    private void BarrelHitChecker()
    {
        if (!game.currentPlayer.BarrelsOn)
        {            
            pinGod.PlayMusic("barrels_loop");
            pinGod.SolenoidOn("vpcoil", 4);

            if (game.currentPlayer.BarrelStage > 0)
            {
                CanSkipScene = true;
                _label.Text = Tr("BARREL_MBALL") + $" {game.currentPlayer.BarrelStage}";
                switch (game.currentPlayer.BarrelStage)
                {
                    case 1:
                        _ballStackPinball.Start(3.3f); //kick the ball after 3.3 secs
                        CallDeferred("PlayScene", "brody_barrel_1");
                        break;

                    case 2:
                        _ballStackPinball.Start(6.2f); //kick the ball after 6.2 secs unless skipped
                        CallDeferred("PlayScene", "brody_barrel_2");
                        break;

                    case 3:
                        _ballStackPinball.Start(3.2f);
                        CallDeferred("PlayScene", "quint_3_barrels");
                        break;
                    default:
                        _label.Text = Tr("BARREL_MBALL_STARTED");
                        CanSkipScene = false;
                        CheckForMultiball();
                        break;
                }

                _label.Visible = true;

                return;
            }
        }

        _ballStackPinball.Start(1f);

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
            _ballStackPinball.Start(1f);
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

        UpdateLamps();
    }

    private void BarrelSceneSkip()
    {
        _ballStackPinball.Stop(); //stop the kicker timer
        CanSkipScene = false;
        game.AddPoints(5000);
        game.currentPlayer.BonusSkipper += game.DoublePlayfield ? Game.BONUS_SKIPPER_VALUE * 2 : Game.BONUS_SKIPPER_VALUE;
        if (!CheckForMultiball())
            _ballStackPinball.SolenoidPulse();
    }

    /// <summary>
    /// Scene to show score
    /// </summary>
    /// <param name="points"></param>
    private void BarrelTargetHit(long points)
    {
        ClearLayerTimer(1.8f);
        _label.Text = points.ToScoreString();        
        CallDeferred("PlayScene", "barrel_shot_" + pinGod.RandomNumber(1, 2));
    }

    /// <summary>
    /// Sets <see cref="game.currentPlayer.BarrelTargetsUp"/> or starts a multiball.
    /// </summary>
    private bool CheckForMultiball()
    {
        CanSkipScene = false;
        if (game.currentPlayer.BarrelsOn) return false;

        var stage = game.currentPlayer.BarrelStage;
        switch (stage)
        {
            case 1:
            case 2:
                game.currentPlayer.BarrelsOn = true;
                pinGod.LogInfo("barrel drop targets enabled");
                ResetDrops();                
                PlayBarrelReminderScene();
                _ballStackPinball.Start(1.8f);
                return false;
            case 3:
                game.currentPlayer.BarrelsOn = true;
                ResetDrops();                
                _label.Text = Tr("BARREL_TARGETS");
                ClearLayerTimer(1.8f);
                _ballStackPinball.Start(1.8f);
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
        game.currentPlayer.BarrelsHitTotal++;
        var score = 100000 * game.currentPlayer.BarrelsHitTotal;
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
        if (IsMulitballRunning)
        {
            IsMulitballRunning = false;
            game.currentPlayer.BarrelsOn = false;
            game.currentPlayer.BarrelStage = 1;
            game.currentPlayer.BarrelMballCurrentScore = 0;
            IsMulitballRunning = false;
            pinGod.LogInfo("barrel: _OnMultiBallEnded");
        }        
    }

    private void GetResources()
    {
		var res = pinGod.GetResources();
        VideoStreams = new Dictionary<string, VideoStreamTheora>();
        VideoStreams.Add("barrel_hook_me_up", res.Resolve("barrel_hook_me_up") as VideoStreamTheora);
        VideoStreams.Add("barrel_round_complete", res.Resolve("barrel_round_complete") as VideoStreamTheora);
		VideoStreams.Add("barrel_shot_1", res.Resolve("barrel_shot_1") as VideoStreamTheora);
        VideoStreams.Add("barrel_shot_2", res.Resolve("barrel_shot_2") as VideoStreamTheora);
		VideoStreams.Add("barrel_start", res.Resolve("barrel_start") as VideoStreamTheora);
		VideoStreams.Add("barrel_tie_it_on", res.Resolve("barrel_tie_it_on") as VideoStreamTheora);
		VideoStreams.Add("brody_barrel_1", res.Resolve("brody_barrel_1") as VideoStreamTheora);
		VideoStreams.Add("brody_barrel_2", res.Resolve("brody_barrel_2") as VideoStreamTheora);
		VideoStreams.Add("quint_3_barrels", res.Resolve("quint_3_barrels") as VideoStreamTheora);
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
	private void KickBall() 
    {
        if (!pinGod.IsMultiballRunning)
            pinGod.SolenoidOn("vpcoil", 0);

        pinGod.SolenoidPulseTimer("barrel_kicker");     
    }
	private void OnBarrelDropsCompleted()
	{
        pinGod.LogInfo("barrel_kicker: incrementing barrel stage to:", game.currentPlayer.BarrelStage + 1);
        game.currentPlayer.BarrelStage++;
        pinGod.LogInfo("barrel drops completed");
		game.ResetBarrelDropsRound(); //reset barrel vars
		ClearLayerTimer(2.5f);
		_label.Text = "BARREL_HOOKED";
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
		_label.Text = Tr("BARREL_MBALL_SHORT");
		pinGod.PlayMusic("barrel_start");
		ClearLayerTimer(3f);
		CallDeferred("PlayScene", "barrel_start");
		game.currentPlayer.BarrelMballCompleteCount++;
		game.UpdateProgress();
	}
}


