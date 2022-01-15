using Godot;
using System;
using System.Collections.Generic;

public class BruceMultiball : Node
{
    #region Properties / Fields
    private bool _hurryupReady;
    private bool _jackpotBusy;
    private Control activatorControl;
    private BlinkingLabel activatorLabel;
    private Timer clearJackpotLayersTimer;
    private Game game;
    bool[] JackportTargets = new bool[2];
    private Timer jawsChompTimer;
    private AudioStreamPlayer _targetAudio;
    private Timer jawsTimer;
    private Timer lockTimer;
    private JawsPinGodGame pinGod;
    private BlinkingLabel superLabel;
    private Timer suspenseTimer;
	public bool CanSkipScene { get; private set; }
	public bool IsMultiballRunning { get; set; }
	public VideoPlayerPinball VideoPlayer { get; private set; }
	public Dictionary<string, VideoStreamTheora> VideoStreams { get; private set; }
	#endregion

	public override void _EnterTree()
    {
        base._EnterTree();
        game = GetParent().GetParent() as Game;
        pinGod = GetNode("/root/PinGodGame") as JawsPinGodGame;
        jawsTimer = GetNode("JawsTimer") as Timer;
        lockTimer = GetNode("LockTimer") as Timer;
        suspenseTimer = GetNode("SuspenseTimer") as Timer;
        jawsChompTimer = GetNode("JawsChompTimer") as Timer;

        _targetAudio = GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));
        _targetAudio.Stream = pinGod.AudioManager.Sfx["shark_hit_fx"];

        clearJackpotLayersTimer = GetNode("CenterContainer/ClearJackpotLayersTimer") as Timer;

        activatorControl = GetNode("Activator") as Control;
        activatorControl.Visible = false;
        activatorLabel = activatorControl.GetNode("CenterContainer/BlinkingLabel") as BlinkingLabel;

        superLabel = GetNode("CenterContainer/BlinkingLabel") as BlinkingLabel;
        superLabel.Text = ""; superLabel.Visible = false;

        VideoPlayer = GetNode("VideoPlayerPinball") as VideoPlayerPinball;

        GetVideoResources();

    }

    public override void _Input(InputEvent @event)
    {
        if (!pinGod.GameInPlay || pinGod.IsTilted) return;

        //lock VUK
        if (pinGod.SwitchOn("bruce_vuk", @event))
        {
            CallDeferred(nameof(BruceVukHit));
        }

        if (_hurryupReady)
        {
            if (pinGod.SwitchOn("inlane_r", @event))
            {
                _hurryupReady = false;
                game.CallDeferred(nameof(game.StartBruceHurryUp));
            }
        }

        //This is the Exit hole when hitting jaws's jaw 
        if (pinGod.SwitchOn("jaws_kicker", @event))
        {
            CallDeferred(nameof(OnJawsKicker));
        }

        if (pinGod.SwitchOn("lock_activate_0", @event))
        {
            CallDeferred(nameof(ActivatorCheck), 0);
        }
        else if (pinGod.SwitchOff("lock_activate_0", @event))
        {
            pinGod.LogInfo("activate off");
        }

        if (pinGod.SwitchOn("lock_activate_1", @event))
        {
            CallDeferred(nameof(ActivatorCheck), 1);
        }
        if (pinGod.SwitchOn("lock_activate_2", @event))
        {
            CallDeferred(nameof(ActivatorCheck), 2);
        }

        //skip mouth and lock scenes
        if (CanSkipScene && !game.IsMultiballScoringStarted)
        {
            if (pinGod.SwitchOn("flipper_l", @event))
            {
                if (pinGod.SwitchOn("flipper_r"))
                {
                    SkipBruceScene();
                }
            }
        }

        if (IsMultiballRunning)
        {
            //Brody_ShotMiss01
            if (pinGod.SwitchOn("jaws_target_left", @event))
            {
                if (!_jackpotBusy)
                {
                    if (!JackportTargets[0])
                    {
                        JackportTargets[0] = true;
                        if (JackportTargets[1])
                        {
                            superLabel.Text = Tr("SUPER_READY");
                            superLabel.Visible = true;
                            pinGod.EnableJawsToy(false);
                            CallDeferred("PlayScene", "brody_miss_01");

                            clearJackpotLayersTimer.Stop();
                            clearJackpotLayersTimer.Start(2f);
                        }
                        else
                        {
                            superLabel.Text = Tr("SHOOT_TARG_R");
                            superLabel.Visible = true;
                            CallDeferred("PlayScene", "brody_miss_01");
                            clearJackpotLayersTimer.Stop();
                            clearJackpotLayersTimer.Start(2f);
                        }

                        UpdateLamps();
                    }
                }
            }
            if (pinGod.SwitchOn("jaws_target_right", @event))
            {
                if (!_jackpotBusy)
                {
                    if (!JackportTargets[1])
                    {
                        clearJackpotLayersTimer.Stop();
                        JackportTargets[1] = true;
                        if (JackportTargets[0])
                        {
                            superLabel.Text = Tr("SUPER_READY");
                            superLabel.Visible = true;
                            pinGod.EnableJawsToy(false);
                            CallDeferred("PlayScene", "brody_miss_01");
                            clearJackpotLayersTimer.Stop();
                            clearJackpotLayersTimer.Start(2f);
                        }
                        else
                        {
                            superLabel.Text = Tr("SHOOT_TARG_L");
                            superLabel.Visible = true;
                            CallDeferred("PlayScene", "brody_miss_02");
                            clearJackpotLayersTimer.Stop();
                            clearJackpotLayersTimer.Start(2f);
                        }

                        UpdateLamps();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Jaws mouth
    /// </summary>
    private void OnJawsKicker()
    {
        if (!IsMultiballRunning)
        {
            pinGod.LogInfo("jaws kicker hit");
            pinGod.EnableJawsToy(true);
            if (game.currentPlayer.SharkLockEnabled)
            {
                JawsLockCheck();
            }
            else
            {
                Kickoutjaws();
            }
        }
        else
        {
            var scored = pinGod.AddPoints(game.currentPlayer.SuperjackpotValue);
            pinGod.DisableAllLamps();
            game.SetGiState(LightState.Blink);
            JackportTargets = new bool[2];
            pinGod.EnableJawsToy(true);
            pinGod.LogInfo("super jackpot");
            _jackpotBusy = true;
            superLabel.Text = Tr("SUPER_JACKPOT") + $"\n{scored.ToScoreString()}";
            superLabel.Visible = true;
            pinGod.PlaySfx("brody_super_cheer");
            CallDeferred("PlayScene", "brody_super_shot");
            jawsTimer.Start(4.4f);
            pinGod.SolenoidOn("vpcoil", 6);
        }
    }

    public void UpdateLamps()
    {
        //activator lamps
        for (int i = 0; i < game.currentPlayer.QuintActivated.Length; i++)
        {
            if (game.currentPlayer.QuintActivated[i]) pinGod.SetLampState("lock_activate_" + i, 1);
            else pinGod.SetLampState("lock_activate_" + i, 0);
        }
        //bruce m-ball lamp
        if (IsMultiballRunning) pinGod.SetLampState("bruce_multiball", 2);
        else if (game.currentPlayer.BarrelMballComplete) pinGod.SetLampState("bruce_multiball", 1);
        else pinGod.SetLampState("bruce_multiball", 0);

        if (IsMultiballRunning)
        {
            //if(JackportTargets[0])
            if (JackportTargets[0]) pinGod.SetLampState("jaws_l", 1);
            else pinGod.SetLampState("jaws_l", 2);

            if (JackportTargets[1]) pinGod.SetLampState("jaws_r", 1);
            else pinGod.SetLampState("jaws_r", 2);
        }

        var locks = game.currentPlayer.SharkLocksComplete;
        if (game.currentPlayer.SharkLockEnabled && locks == 0)
        {
            pinGod.SetLampState("bruce_0", 2);
        }
        else if (locks == 1)
        {
            pinGod.SetLampState("bruce_0", 1);

            if (game.currentPlayer.SharkLockEnabled)
            {
                pinGod.SetLampState("bruce_1", 2);
                pinGod.SetLampState("shark_lock_bulb", 1);
            }
            else
            {
                pinGod.SetLampState("bruce_1", 0);
                pinGod.SetLampState("shark_lock_bulb", 0);
            }
        }
        else if (locks == 2)
        {
            pinGod.SetLampState("bruce_0", 1);
            pinGod.SetLampState("bruce_1", 1);
            if (game.currentPlayer.SharkLockEnabled)
            {
                pinGod.SetLampState("bruce_2", 2);
                pinGod.SetLampState("shark_lock_bulb", 1);
            }
            else
            {
                pinGod.SetLampState("bruce_2", 0);
                pinGod.SetLampState("shark_lock_bulb", 0);
            }
        }
        else if (locks == 3)
        {
            pinGod.SetLampState("bruce_0", 1);
            pinGod.SetLampState("bruce_1", 1);
            pinGod.SetLampState("bruce_2", 1);
        }

        //the lamps to the right of playfield
        var quintOn = game.currentPlayer.QuintLockEnabled;
        switch (game.currentPlayer.QuintLocksComplete)
        {
            case 0:
                if (quintOn)
                    pinGod.SetLampState("quint_0", 2);
                break;
            case 1:
                pinGod.SetLampState("quint_0", 1);
                if (quintOn)
                    pinGod.SetLampState("quint_1", 2);
                break;
            case 2:
                pinGod.SetLampState("quint_0", 1);
                pinGod.SetLampState("quint_1", 1);
                if (quintOn)
                    pinGod.SetLampState("quint_2", 2);
                break;
            case 3:
                pinGod.SetLampState("quint_0", 1);
                pinGod.SetLampState("quint_1", 1);
                pinGod.SetLampState("quint_2", 1);
                break;
            default:
                break;
        }
    }

    private void _on_ClearJackpotLayersTimer_timeout()
    {
        superLabel.Visible = false;
    }

    private void _on_JawsChompTimer_timeout()
    {        
        if (pinGod.JawsToyState > 0)
        {
            pinGod.EnableJawsToy(false);
            pinGod.JawsToyState = 0;
        }            
        else
        {
            pinGod.JawsToyState = 1;
            pinGod.EnableJawsToy(true);
        }            
    }

    private void _on_JawsTimer_timeout()
    {
        Kickoutjaws();
    }

    private void _on_LockTimer_timeout()
    {
        int lockNum = PlayHurryUpSuspense();

        pinGod.LogInfo("jaws open:" + game.IsJawsLockReady());
        pinGod.SolenoidOn("flash_jaws", 1);
        superLabel.Text = $"\r\n\r\n{Tr("SHOOT_SHARK")}"; superLabel.Visible = true;
        suspenseTimer.Start(3.2f);
        _hurryupReady = true;
        KickoutLock();
        pinGod.SolenoidOn("vpcoil", 0);
        switch (lockNum)
        {
            case 1:
                CallDeferred("PlayScene", "quint_lock_1b");
                break;
            case 2:
                CallDeferred("PlayScene", "quint_lock_2b");
                break;
            case 3:
                CallDeferred("PlayScene", "quint_lock_3b");
                break;
            default:
                break;
        }
    }

    private void _on_RemoveActivatorTimer_timeout()
    {
        activatorControl.GetNode<AnimatedSprite>("TextureRect").Stop();
        activatorControl.Visible = false;        
    }

    private void _on_SuspenseTimer_timeout()
    {
        superLabel.Visible = false;
        game.UpdateLamps();
        pinGod.PlayMusic("suspense_loop");
    }

    /// <summary>
    /// Activator locks for burce. Will return if multiball is running
    /// </summary>
    /// <param name="index"></param>
    void ActivatorCheck(int index)
    {
        game.AddPoints(7500);
        game.currentPlayer.Bonus += Game.BONUS_VALUE;
        if (IsMultiballRunning)
            return;

        var quintOn = game.currentPlayer.QuintLockEnabled;
        var sharkOn = game.currentPlayer.SharkLockEnabled;

        pinGod.SolenoidPulseTimer("flash_lock");
        _targetAudio.Play();

        if (quintOn || sharkOn)
            return;
        else
        {
            game.AddPoints(7500);
            pinGod.LogInfo("activated");
            if (!game.currentPlayer.QuintActivated[index])
            {
                game.currentPlayer.QuintActivated[index] = true;
            }

            var completed = game.currentPlayer.IsActivatorComplete();
            //scene: bStars, sharktoon
            if (completed)
            {
                pinGod.LogInfo("activator completed");
                game.currentPlayer.QuintLockEnabled = true;
                activatorLabel.Text = Tr("BRUCE\r\nLOCKS OPEN");
                if (!pinGod.IsMultiballRunning || game.currentPlayer.BarrelsOn)
                {
                    pinGod.PlayMusic("dundun_1");
                }

                pinGod.SolenoidOn("diverter", 1);
            }
            else
            {
                activatorLabel.Text = Tr("BRUCE\r\nACTIVATOR");
            }

            activatorControl.Visible = true;
            activatorControl.GetNode<AnimatedSprite>("TextureRect").Play();
            var timer = activatorControl.GetNode("RemoveActivatorTimer") as Timer;
            timer.Stop();
            timer.Start(1.2f);
            UpdateLamps();
        }
    }

    private void BruceVukHit()
    {
        pinGod.LogInfo("bruce kicker");
        if (pinGod.IsMultiballRunning)
        {
            pinGod.LogInfo("bruce: in multiball exiting");
            game.currentPlayer.BonusBruce += game.DoublePlayfield ? Game.BONUS_BRUCEY_VALUE * 2 : Game.BONUS_BRUCEY_VALUE;
            KickoutLock();
        }
        else if (game.currentPlayer.BarrelsOn)
        {
            pinGod.LogInfo("bruce: targets up exiting");
            game.PlayBarrelReminderScene();
            KickoutLock();
        }
        else
        {
            pinGod.LogInfo("bruce: checking quint");
            QuintLockCheck();
        }
    }

    private void EndMultiball()
    {
        if (IsMultiballRunning)
        {
            IsMultiballRunning = false;
            game.currentPlayer.SharkLockEnabled = false;
            game.currentPlayer.SharkLocksComplete = 0;
            game.currentPlayer.QuintCount = 0;
            pinGod.EnableJawsToy(true);
            superLabel.Visible = false;
            superLabel.Text = "";
            UpdateLamps();
            pinGod.LogInfo("bruce: EndMultiball");
        }
    }

    private void GetVideoResources()
    {
        try
        {
            var res = pinGod.GetResources();
            VideoStreams = new Dictionary<string, VideoStreamTheora>();
            VideoStreams.Add("quint_lock_1a", res.Resolve("quint_lock_1a") as VideoStreamTheora);
            VideoStreams.Add("quint_lock_2a", res.Resolve("quint_lock_2a") as VideoStreamTheora);
            VideoStreams.Add("quint_lock_3a", res.Resolve("quint_lock_3a") as VideoStreamTheora);
            VideoStreams.Add("quint_lock_1b", res.Resolve("quint_lock_1b") as VideoStreamTheora);
            VideoStreams.Add("quint_lock_2b", res.Resolve("quint_lock_2b") as VideoStreamTheora);
            VideoStreams.Add("quint_lock_3b", res.Resolve("quint_lock_3b") as VideoStreamTheora);
            VideoStreams.Add("quint_dead", res.Resolve("quint_dead") as VideoStreamTheora);
            VideoStreams.Add("brody_bruce_start", res.Resolve("brody_bruce_start") as VideoStreamTheora);
            VideoStreams.Add("brody_miss_01", res.Resolve("brody_miss_01") as VideoStreamTheora);
            VideoStreams.Add("brody_miss_02", res.Resolve("brody_miss_02") as VideoStreamTheora);
            VideoStreams.Add("brody_super_shot", res.Resolve("brody_super_shot") as VideoStreamTheora);
        }
        catch (Exception ex)
        {
            pinGod.LogError($"get video resources failed. {ex}");
        }
    }

    /// <summary>
    /// When player hits the Jaws mouth
    /// </summary>
    private void JawsLockCheck()
    {
        if (game.currentPlayer.SharkLockEnabled)
        {
            pinGod.LogInfo("bruce: jaws lock check. shark complete:", game.currentPlayer.SharkLocksComplete);
            pinGod.SolenoidOn("flash_vuk", 1);
            pinGod.SolenoidOn("flash_jaws", 0);
            game.AwardHurryUp();
            jawsTimer.Stop();

            if (game.currentPlayer.SharkLocksComplete < 2)
            {
                game.AddPoints(100000);
                pinGod.SolenoidOn("diverter", 1);
                game.currentPlayer.SharkLockEnabled = false;
                game.currentPlayer.QuintLockEnabled = true;
                game.IsJawsLockReady();

                if (game.currentPlayer.SharkLocksComplete == 0)
                    game.currentPlayer.SharkLocksComplete++;
                else
                    game.currentPlayer.SharkLocksComplete = 2;

                pinGod.PlaySfx("shark_hit_fx");
                pinGod.DisableAllLamps();
                game.SetGiState(LightState.On);                
                jawsTimer.Start(2.0f);
            }
            else
            {
                pinGod.AudioManager.StopMusic();

                game.currentPlayer.SharkLocksComplete = 3;
                CanSkipScene = true;
                jawsTimer.Start(11.8f);
                jawsChompTimer.Start(0.8f);
                superLabel.Text = $"\r\n\r\n{Tr("BRUCE_MBALL")}"; superLabel.Visible = true; //TODO: scene timer which displays what to do in multiball
                CallDeferred("PlayScene", "quint_dead");
                pinGod.SolenoidOn("vpcoil", 7);
            }

            pinGod.LogInfo("bruce: jaws lock checked. shark complete:", game.currentPlayer.SharkLocksComplete);
        }
        else
        {
            Kickoutjaws();
        }
    }

    /// <summary>
    /// Starting multiball?
    /// </summary>
    private void Kickoutjaws()
    {
        pinGod.SolenoidOn("flash_vuk", 0);
        pinGod.SolenoidOn("flash_jaws", 0);

        game.currentPlayer.Bonus += Game.BONUS_VALUE;
        game.currentPlayer.BonusBruce += game.DoublePlayfield ? Game.BONUS_BRUCEY_VALUE * 2 : Game.BONUS_BRUCEY_VALUE;

        if (!IsMultiballRunning)
        {
            game.AwardHurryUp();
            game.StopHurryUp();
            game.currentPlayer.SharkLockEnabled = false;

            //starting multiball
            if (game.currentPlayer.SharkLocksComplete == 3)
            {
                jawsChompTimer.Stop();
                IsMultiballRunning = true;
                game.currentPlayer.BruceMballCompleteCount++;
                game.UpdateProgress();

                pinGod.AudioManager.StopMusic();
                pinGod.PlayMusic("end_music_final");
                game.AddMultiScoringMode();
                pinGod.StartMultiBall(4, 20);
                game.currentPlayer.ResetBruce();
                pinGod.EnableJawsToy(true);
                pinGod.SolenoidPulseTimer("jaws_kicker");
                superLabel.Text = $"\r\n\r\n{Tr("BRUCE_MBALL")}"; ; superLabel.Visible = true; //TODO: scene timer which displays what to do in multiball
                CallDeferred("PlayScene", "brody_bruce_start");
            }
            else
            {
                game.PlayMusicForMode();
                pinGod.SolenoidPulse("jaws_kicker");
            }
            game.UpdateLamps();
        }
        else
        {
            _jackpotBusy = false;
            superLabel.Visible = false;

            game.UpdateLamps();
            pinGod.SolenoidOn("flash_jaws", 0);
            pinGod.SolenoidPulseTimer("jaws_kicker");
            pinGod.SolenoidOn("vpcoil", 0);
        }
    }

    private void KickoutLock()
    {
        pinGod.SolenoidPulseTimer("bruce_vuk");
    }

    private int PlayHurryUpSuspense()
    {
        var lockNum = game.currentPlayer.QuintLocksComplete;
        if (lockNum < 3)
        {
            pinGod.PlayMusic("music_suspense_end");
        }

        return lockNum;
    }

    private void PlayScene(string name)
    {
        activatorControl.Visible = false;
        pinGod.LogInfo("playing scene: ", name);
        VideoPlayer.Stream = VideoStreams[name];
        VideoPlayer.Play();
        VideoPlayer.Visible = true;
    }

    void QuintLockCheck()
    {
        var quinkLocksOn = game.currentPlayer.QuintLockEnabled;
        var quinkLocksComplete = game.currentPlayer.QuintLocksComplete;
        game.currentPlayer.BonusBruce += game.DoublePlayfield ? Game.BONUS_BRUCEY_VALUE * 2 : Game.BONUS_BRUCEY_VALUE;
        if (quinkLocksOn)
        {
            pinGod.LogInfo("quint check locks on");
            pinGod.DisableAllLamps();
            game.SetGiState(LightState.On);
            game.currentPlayer.BonusBruce += game.DoublePlayfield ? Game.BONUS_BRUCEY_VALUE * 2 : Game.BONUS_BRUCEY_VALUE;

            superLabel.Text = $"\r\n\r\n{Tr("BRUCE_LOCK")} {quinkLocksComplete+1}"; superLabel.Visible = true;
            switch (quinkLocksComplete)
            {
                case 0:
                    game.currentPlayer.QuintLocksComplete++;
                    lockTimer.Start(4.3f);
                    CanSkipScene = true;                    
                    CallDeferred("PlayScene", "quint_lock_1a");
                    break;
                case 1:
                    lockTimer.Start(5f);
                    CanSkipScene = true;
                    game.currentPlayer.QuintLocksComplete++;
                    CallDeferred("PlayScene", "quint_lock_2a");
                    break;
                case 2:
                    game.currentPlayer.QuintLocksComplete++;
                    lockTimer.Start(4.8f);
                    CanSkipScene = true;
                    CallDeferred("PlayScene", "quint_lock_3a");
                    break;
                default:
                    break;
            }

            game.currentPlayer.QuintLockEnabled = false;
            game.currentPlayer.SharkLockEnabled = true;
            pinGod.SolenoidOn("vpcoil", 3);
        }
        else
        {
            pinGod.LogInfo("quint check locks off");
            KickoutLock();
        }
    }

    private void SkipBruceScene()
    {
		CanSkipScene = false;
		game.currentPlayer.BonusSkipper += game.DoublePlayfield ? Game.BONUS_SKIPPER_VALUE * 2 : Game.BONUS_SKIPPER_VALUE;        
		lockTimer.Stop();        
		game.AddPoints(7500, false);
		if (pinGod.SwitchOn("bruce_vuk"))
		{
			CallDeferred(nameof(_on_LockTimer_timeout));
		}
		else
		{
			Kickoutjaws();
		}
	}
}
