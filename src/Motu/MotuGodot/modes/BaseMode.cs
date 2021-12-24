using Godot;
using System.Linq;

public class BaseMode : Control
{
    public Timer _comboTimer;
    private AnimatedSprite _anims;
    private PackedScene _beastmanMode;
    private AudioStreamPlayer _bumperAudio;
    string[] _bumpers = new string[] { "bumperl", "bumperm", "bumperr" };
    private Timer _clearDisplayTimer;
    private Control _control;
    private Label _label;
    private Label _label2;
    private Timer _launchBallTimer;
    private MotuPlayer _player;
    private AudioStreamPlayer _skeleAudio;
    private PackedScene _skeleMultiball;
    [Export] Godot.Collections.Array<AudioStream> _skeletorStreams;
    private AudioStreamPlayer _spinAudio;
    private PackedScene _stratosMode;
    string[] ComboLamps = new string[] { "arrow_l_left_loop", "arrow_l_left_ramp", "arrow_l_inner_spin", "arrow_l_inner_loop", "arrow_l_right_ramp", "arrow_l_right_loop" };
    private Game game;
    private ManAtArms manAtArmsMode;
    internal MultiballGrayskull grayskullMball;
    internal MultiballRipper ripperMball;
    private PinGodGame pinGod;
    string[] RipperLamps = new string[] { "rip_left_loop", "rip_ramp_left", "rip_inner_spin", "rip_inner_right", "rip_right_ramp", "rip_right_loop" };
    private BeastmanMode beastModeInstance;
    private StratosMode stratosModeInstance;

    public override void _EnterTree()
    {
        pinGod = GetNode("/root/PinGodGame") as PinGodGame;
        game = GetParent().GetParent() as Game;       

        var mainScene = GetNodeOrNull<MotuMainScene>("/root/MainScene");
        if (mainScene == null) //for testing when main scene isn't running
        {
            _beastmanMode = ResourceLoader.Load("res://modes/BeastmanMode.tscn") as PackedScene;
            _stratosMode = ResourceLoader.Load("res://modes/StratosMode.tscn") as PackedScene;
            _skeleMultiball = ResourceLoader.Load("res://modes/MultiballSkeletor.tscn") as PackedScene;
        }
        else //get the packed scenes from main
        {
            _beastmanMode = mainScene._beastmanMode;
            _stratosMode = mainScene._stratosMode;
            _skeleMultiball = mainScene._skeleMultiball;
        }

        _bumperAudio = GetNode<AudioStreamPlayer>("BumperAudioStream");
        _skeleAudio = GetNode<AudioStreamPlayer>("SkeleAudioStream");
        _spinAudio = GetNode<AudioStreamPlayer>("SpinnerAudioStream");

        _control = GetNode<Control>("Control");
        _anims = _control.GetNode<AnimatedSprite>("Anims");
        _label = _control.GetNode<Label>("Label");
        _label2 = _control.GetNode<Label>("Label2");

        _clearDisplayTimer = GetNode<Timer>("ClearDisplayTimer");
        _launchBallTimer = GetNode<Timer>("LaunchBallTimer");
        _comboTimer = GetNode<Timer>("ComboTimer");

        manAtArmsMode = GetParent().GetNode<ManAtArms>("ManAtArms");

        //grayskullMball = GetParent().GetNode<MultiballGrayskull>("MultiballGrayskull");
        //ripperMball = GetParent().GetNode<MultiballRipper>("MultiballRipper");
    }

    /// <summary>
    /// Basic input switch handling
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (!pinGod.GameInPlay) return;
        if (pinGod.IsTilted) return; //game is tilted, don't process other switches when tilted
        if (_player == null) return; //wait till player is in the game

        if (pinGod.SwitchOn("start", @event))
        {
            pinGod.LogInfo("base: start button adding player...", pinGod.StartGame());
        }

        if (!pinGod.IsBallStarted) return;

        // Skillshot with flipper controlling the post. Only if the ball isn't started and the plunger lane active
        if (pinGod.SwitchOn("flipper_l", @event))
        {
            if (!pinGod.BallStarted && pinGod.SwitchOn(pinGod._trough._plunger_lane_switch))
            {
                pinGod.SolenoidOn("post", 0);
            }
        }
        else if (pinGod.SwitchOff("flipper_l", @event))
        {
            if (!pinGod.BallStarted && pinGod.SwitchOn(pinGod._trough._plunger_lane_switch))
            {
                pinGod.SolenoidOn("post", 1);
            }
        }

        if (pinGod.SwitchOn("flipper_r", @event))
        {
        }
        if (pinGod.SwitchOn("outlane_l", @event))
        {
            pinGod.AddPoints(MotuConstant._5K);
            if (!pinGod.BallSaveActive)
            {
                PlaySkeleDrainSound();
            }
        }
        if (pinGod.SwitchOn("outlane_r", @event))
        {
            pinGod.AddPoints(MotuConstant._5K);
            if (!pinGod.BallSaveActive)
            {
                PlaySkeleDrainSound();
            }
        }
        if (pinGod.SwitchOn("sling_l", @event))
        {
            pinGod.AddPoints(MotuConstant._1K);
            pinGod.PlaySfx("sfx3");
        }
        if (pinGod.SwitchOn("sling_r", @event))
        {
            pinGod.AddPoints(MotuConstant._1K);
            pinGod.PlaySfx("sfx3");
        }

        if (!pinGod.BallStarted)
        {
            //handle plunger lane on
            if (pinGod.SwitchOn(pinGod._trough._plunger_lane_switch, @event))
            {
                OnPlungerSwitch();
            }

            //handle plunger lane off
            if (pinGod.SwitchOff(pinGod._trough._plunger_lane_switch, @event))
            {
                _launchBallTimer.Start();                
            }
        }

        //check bumper array
        if (pinGod.IsSwitch(_bumpers, @event))
        {
            OnBumperHit();
        }

        //escape all swiitches, we want player to shoot scoop
        if (_player.IsSorceressReady()) return;

        //see these modes _inputs when running
        if (!_player.IsSorceressRunning && !_player.IsMotuMultiballRunning)
        {
            if (pinGod.SwitchOn("heman_lane", @event))
            {
                pinGod.AddPoints(MotuConstant._1K);
                if (!_player.IsBeastModeRunning)
                {
                    CallDeferred(nameof(StartBeastman));
                }
            }

            if (!_player.IsAnyMultiballRunning())
            {
                //only allow skele switch when roton ready and open
                if (_player.IsRotonReady)
                {
                    var lastActive = pinGod.GetLastSwitchChangedTime("skele_trigger");
                    if (pinGod.SwitchOn("skele_trigger", @event))
                    {
                        if (lastActive > 1200f || lastActive == 0)
                        {
                            IncrementSkeletorTriggerHit();
                        }
                    }
                }
                else
                {
                    //Skeletor switches
                    if (pinGod.SwitchOn("skele_l", @event))
                    {
                        pinGod.AddPoints(1090);
                        AdvanceRoton(0);
                    }
                    if (pinGod.SwitchOn("skele_m", @event))
                    {
                        pinGod.AddPoints(1090);
                        AdvanceRoton(1);
                    }
                    if (pinGod.SwitchOn("skele_r", @event))
                    {
                        pinGod.AddPoints(1090);
                        AdvanceRoton(2);
                    }
                }
            }

            if (pinGod.SwitchOn("loop_left", @event))
            {
                OnLoopLeft();
            }

            if (pinGod.SwitchOn("leftramp_full", @event))
            {
                OnLeftRamp();
            }

            var spinLastChange = pinGod.GetLastSwitchChangedTime("inner_spinner");
            if (pinGod.SwitchOn("inner_spinner", @event))
            {
                OnSpinnerHit(spinLastChange);
            }

            spinLastChange = pinGod.GetLastSwitchChangedTime("spinner_r");
            if (pinGod.SwitchOn("spinner_r", @event))
            {
                if (pinGod.IsBallStarted)
                {
                    pinGod.AddPoints(MotuConstant._1K);
                    _spinAudio.Play();

                    if (spinLastChange > 2000f)
                    {
                        //stratos
                        if (_player.IsStratosRunning)
                        {
                            stratosModeInstance?.AddTime();
                        }
                    }
                }
            }

            //stratos
            if (pinGod.SwitchOn("inner_skele_loop", @event))
            {
                OnInnerSkeleLoop();
            }

            if (pinGod.SwitchOn("rip_ramp_mid", @event))
            {
                OnRightRamp();
            }

            if (pinGod.SwitchOn("loop_right", @event))
            {
                OnLoopRight();
            }
        }        
                
    }

    RandomNumberGenerator rand = new RandomNumberGenerator();
    private void PlaySkeleDrainSound()
    {
        game.StopSkeleSounds();
        var i = rand.RandiRange(5, 8);
        _skeleAudio.Stream = _skeletorStreams[i];
        _skeleAudio.Play();
    }

    public void AdvanceRoton(int target)
    {
        if (pinGod.IsMultiballRunning) return;
        if (_player.IsRotonReady) return;

        pinGod.LogInfo("advancing roton");
        for (int i = 0; i < _player.RotonTargets.Length; i++)
        {
            if (!_player.RotonTargets[i])
            {
                _player.RotonTargets[i] = true;
                _player.SetLadder("roton", MotuLadder.PartComplete);
                break;
            }
        }

        if (!_player.RotonTargets.Any(x => !x))
        {
            pinGod.LogInfo("roton targets completed. opening skeletor shot");
            _player.IsRotonReady = true;
            _player.SetLadder("roton", MotuLadder.Complete);
            game.StartSorceress();
            _player.RotonTargets = new bool[4];

            (pinGod as MotuPinGodGame).SkellyTargetBankController();

            //todo: play sound HeMan_SkeleOpened

            _skeleAudio.Stream = _skeletorStreams[0];
            _skeleAudio.Play();

            pinGod.PlaySfx("skele_coming");
            pinGod.PlayMusic("creeping");

            pinGod.AddPoints(MotuConstant._200K); //dropping the targets score
            UpdateLamps();
        }
        else
        {
            pinGod.LogInfo("roton targets not complete");
            //todo: sound SkeleRoton stop and play, laser07
            //todo: display skele remaining, seq SkeleTargetHit

            //_skeleAudio.Stream = "machine, steelgate"
            _skeleAudio.Stream = _skeletorStreams[2];
            _skeleAudio.Play();
        }
    }

    public void OnBallDrained()
    {
        pinGod.DisableAllLamps();
        pinGod.SolenoidOn("vpcoil", 1);
        _skeleAudio.Stream = _skeletorStreams[4];
        _skeleAudio.Play();
        PlayThenClearAnimation("skele_intro", _player.Bonus.ToScoreString(), "BONUS");
    }

    public void OnBallSaved()
    {
        pinGod.LogDebug("base: ball_saved");
        if (!pinGod.IsMultiballRunning)
        {
            pinGod.LogDebug("ballsave: ball_saved");
            pinGod.PlaySfx("ram_back");
            PlayThenClearAnimation("ram_back", "BALL SAVE", "DON'T MOVE RAM MAN SAVED");
            pinGod.SolenoidOn("vpcoil", 5);
        }
        else
        {
            pinGod.LogDebug("skipping save display in multiball");
        }
    }

    public void OnBallStarted()
    {
        _player = (pinGod.Player as MotuPlayer);
        _player.IsSkillshotRunning = true;
        _control.Visible = false; //hide the layers

        if(!game.StartSorceress())
            (pinGod as MotuPinGodGame).SkellyTargetBankController();

        _player.IsMotuMultiballRunning = false;
    }

    public void UpdateLamps()
    {
        if (_player.IsMotuMultiballRunning || _player.IsWizardReady || _player.IsSorceressRunning) return;

        //combo arrows. todo: switch off after 2.5 secs
        var combo = (byte)(_player.IsComboAvailable ? 2 : 0);
        foreach (var lamp in ComboLamps)
        {
            pinGod.SetLampState(lamp, combo);
        }

        //set lamps to blink if part and set on if fully completed
        foreach (var mode in _player.MotuLadderComplete)
        {
            switch (mode.Value)
            {
                case MotuLadder.None:
                    pinGod.SetLampState("mode_" + mode.Key, 0);
                    break;
                case MotuLadder.PartComplete:
                    pinGod.SetLampState("mode_" + mode.Key, 2);
                    break;
                case MotuLadder.Complete:
                    pinGod.SetLampState("mode_" + mode.Key, 1);
                    break;
                default:
                    break;
            }
        }

        if (!_player.IsSorceressRunning)
        {
            //set multiballs ready lamps
            if (_player.IsGraySkullReady || _player.IsGrayskullMultiballRunning) pinGod.SetLampState("gray_ready", 1);
            else pinGod.SetLampState("gray_ready", 0);
            if (_player.IsRipperMultiballReady || _player.IsRipperMultiballRunning) pinGod.SetLampState("rip_ready", 1);
            else pinGod.SetLampState("rip_ready", 0);

            //mystery / he-man lane
            if (_player.IsMysteryReady)
            {
                pinGod.SetLampState("heman_lane", 1);
                pinGod.SetLampState("mystery", 1);
            }
            else
            {
                pinGod.SetLampState("heman_lane", 0);
                pinGod.SetLampState("mystery", 0);
            }

            //ripper shots
            if (!_player.IsRipperMultiballRunning)
            {
                for (int i = 0; i < _player.RipperShots.Length; i++)
                {
                    pinGod.SetLampState(RipperLamps[i], (byte)(_player.RipperShots[i] ? 0 : 1));
                }
            }

            //gray skull spinner and ready
            if (!_player.IsGrayskullMultiballRunning)
            {
                if (!_player.GrayskullProgress.Any(x => !x))
                {
                    pinGod.SetLampState("gray_spin", 0);
                }
                else
                {
                    pinGod.SetLampState("gray_spin", 2);
                }
            }
            else
            {
                pinGod.SetLampState("gray_spin", 2);
            }

            if (_player.IsWizardReady)
            {
                //todo: flash coil, blink slow
                pinGod.SetLampState("motu_ready", 2);
            }
            else pinGod.SetLampState("motu_ready", 0);
        }
    }

    internal void StopLaunchBallTimer()
    {
        _launchBallTimer.Stop();
        SetBallStarted();
    }

    private void CheckForCombo()
    {
        if (_player.CheckForComboStatus())
        {
            if (!_comboTimer.IsStopped())
            {
                _comboTimer.Stop();
            }

            _comboTimer.Start();
        }
        else
        {
            if (_player.BankedCombos > 2)
            {
                pinGod.PlaySfx("combo3");
            }
            if (_player.BankedCombos > 1)
            {
                pinGod.PlaySfx("combo2");
            }
            else
            {
                pinGod.PlaySfx("combo1");
            }
        }
    }

    private void CheckRipperShot(int shotLaneNum)
    {
        if (_player.CheckForRipperShot(shotLaneNum))
        {
            if (_player.IsRipperMultiballReady)
            {
                PlayThenClearAnimation("rip_skillshot", "RIPPER MULTIBALL", "SHOOT HE-MAN SCOOP TO START MULTIBALL");
            }
            else
            {
                var shotCnt = _player.RipperShots.Count(x => x);
                PlayThenClearAnimation("rip_skillshot", "RIPPER MULTIBALL", shotCnt + " SHOTS MADE");
            }
        }
    }

    void ClearDisplayTimer_timeout()
    {
        _control.Visible = false;
        _anims.Stop();
        if (!pinGod.IsMultiballRunning)
        {
            pinGod.DisableAllLamps();
            UpdateLamps();
            //turn off lamp sequencing
            pinGod.SolenoidOn("vpcoil", 0);
        }
    }

    void ComboTimer_timeout()
    {
        _player.ResetCombo();
    }

    private void IncrementSkeletorTriggerHit()
    {
        _player.SkeletorHits++;
        pinGod.LogInfo("skele trigger hit: " + _player.SkeletorHits);
        pinGod.PlaySfx("horsefx_1");
        if (_player.SkeletorHits >= _player.SkeletorHitTarget)
        {
            CallDeferred(nameof(StartSkeletorMultiball));
        }
        else
        {
            string txt = "";
            var remainingShots = _player.SkeletorHitTarget - _player.SkeletorHits;
            if (remainingShots == 1)
            {
                //todo: flash skeletor fast
                txt = $"MULTIBALL READY";
                pinGod.SolenoidOn("vpcoil", 5); //blink GI and flashers
            }
            else
            {
                txt = $"{remainingShots} SHOTS REMAIN";
                pinGod.SolenoidOn("vpcoil", 4); //blink GI and flashers
            }

            PlayThenClearAnimation("skele_shot", txt, "shoot skeletor for multiball");
        }
    }

    void LaunchBallTimer_timeout()
    {
        SetBallStarted();
    }

    private void OnBumperHit()
    {
        //disable skillshot
        if (_player.IsSkillshotRunning)
        {
            _player.IsSkillshotRunning = false;
        }

        if (_player.IsAdamSuperBumperRunning)
        {
            _player.SuperBumperScore += MotuConstant._1K;
            pinGod.AddPoints(_player.SuperBumperScore);
        }
        else
        {
            pinGod.AddPoints(MotuConstant._1K);
        }

        if (!_bumperAudio.Playing)
            _bumperAudio.Play();
    }

    private void OnInnerSkeleLoop()
    {
        CheckForCombo();

        if (!_player.IsSorceressRunning && !_player.IsMotuMultiballRunning)
        {
            pinGod.AddPoints(MotuConstant._25K);

            //beastman
            if (_player.IsBeastModeRunning && _player.BeastActiveTargets[3])
            {
                beastModeInstance?.FoundBeastman();
            }

            //ripper
            if (_player.IsRipperMultiballRunning)
            {
                ripperMball.RipperShotHit(3);
            }
            else
            {
                var ripShotComplete = _player.CheckForRipperShot(3);
            }

            //stratos
            if (_player.IsStratosRunning)
            {
                stratosModeInstance?.ProcessStratosShot();
            }

            if (!_player.IsStratosRunning)
            {
                _player.AdvanceStratos();
                if (_player.IsStratosRunning)
                {
                    CallDeferred(nameof(StartStratos));
                }
            }

            //grayskull super jackpot
            if (_player.IsGrayskullMultiballRunning)
            {
                grayskullMball.CollectSuperJackpot();
            }
        }

        UpdateLamps();
    }

    private void OnLeftRamp()
    {       
        if (!_player.IsSorceressRunning)
        {
            pinGod.AddPoints(MotuConstant._1K);
            CheckForCombo();

            if (_player.IsSkillshotRunning)
            {
                pinGod.AddPoints(_player.SkillshotRampLeftAward);
                _player.IsSkillshotRunning = false;
                PlayThenClearAnimation("rip_skillshot", "SKILLSHOT", _player.SkillshotRampLeftAward.ToScoreString());
            }

            //beastman
            if (_player.IsBeastModeRunning && _player.BeastActiveTargets[1])
            {
                beastModeInstance?.FoundBeastman();
            }

            if (_player.IsRipperMultiballRunning)
            {
                ripperMball.RipperShotHit(1);
                ripperMball.LeftRamp();
            }
            else
            {
                _player.CheckForRipperShot(1);
                UpdateLamps();
            }
        
        }    
    }

    private void OnLoopLeft()
    {
        var lastChange = pinGod.GetLastSwitchChangedTime("loop_right");
        if (lastChange > 2000f)
        {
            CheckForCombo();

            if (!_player.IsSorceressRunning)
            {
                pinGod.AddPoints(MotuConstant._1K);

                if (_player.IsSkillshotRunning)
                {
                    _player.SkillshotRightRampOn = true;
                }

                //beastman
                if (_player.IsBeastModeRunning && _player.BeastActiveTargets[0])
                {
                    beastModeInstance?.FoundBeastman();
                }

                //ripper
                if (_player.IsRipperMultiballRunning)
                {
                    ripperMball.RipperShotHit(0);
                }
                else
                {
                    CheckRipperShot(0);
                }

                //manatarms
                if (_player.IsManAtArmsRunning)
                {
                    if (!_player.ManAtArmsProgress[0])
                    {
                        _player.ManAtArmsProgress[0] = true;
                        manAtArmsMode.CheckRunningProgressComplete(manAtArmsMode.AwardCurrentLitScore());
                    }
                }
            }

            UpdateLamps();
        }
    }

    private void OnLoopRight()
    {
        var loopChange = pinGod.GetLastSwitchChangedTime("loop_left");
        if (loopChange < 3000f || pinGod.GetLastSwitchChangedTime("plunger_lane") < 3000f) { }
        else
        {
            if (!_player.IsSorceressRunning)
            {
                pinGod.AddPoints(MotuConstant._1K);

                //beastman
                if (_player.IsBeastModeRunning && _player.BeastActiveTargets[5])
                {
                    beastModeInstance?.FoundBeastman();
                }

                if (_player.IsRipperMultiballRunning)
                {
                    ripperMball.RipperShotHit(5);
                }
                else
                {
                    if (!_player.CheckForRipperShot(5))
                    {
                        //todo: scene TroughCatRun
                    }
                }                

                if (!_player.IsMysteryReady && !_player.IsAnyMultiballRunning())
                {
                    _player.IsMysteryReady = true;
                    //todo: say something
                }

                if (_player.IsManAtArmsRunning && !_player.ManAtArmsProgress[2])
                {
                    _player.ManAtArmsProgress[2] = true;
                    manAtArmsMode.CheckRunningProgressComplete(manAtArmsMode.AwardCurrentLitScore());
                }

                CheckForCombo();
                UpdateLamps();
            }
        }
    }

    private void OnPlungerSwitch()
    {
        _anims.Animation = "plunger_lane"; _anims.Frame = 0;
        _anims.Play();
        _label.Text = $"PLAYER {pinGod.CurrentPlayerIndex + 1} SKILLSHOT";
        _label2.Text = $"Left {_player.SkillshotRampLeftAward.ToScoreString()} Right {_player.SkillshotRampRightAward.ToScoreString()} He-Man {_player.SkillshotScoopAward.ToScoreString()}";
        _control.Visible = true;
        if (pinGod.SwitchOn("flipper_l"))
        {
            pinGod.SolenoidOn("post", 0);
        }
        else
        {
            pinGod.SolenoidOn("post", 1);
        }
        pinGod.PlayMusic("saving_day_loop");
    }

    private void OnRightRamp()
    {
        if (!_player.IsSorceressRunning && !_player.IsMotuMultiballRunning)
        {
            pinGod.AddPoints(MotuConstant._1K);
            CheckForCombo();

            if (!_player.IsAnyMultiballRunning())
            {
                pinGod.DisableAllLamps();
                pinGod.SolenoidOn("vpcoil", 5); //todo: diff lampshow
            }

            //beastman
            if (_player.IsBeastModeRunning && _player.BeastActiveTargets[4])
            {
                beastModeInstance?.FoundBeastman();
            }

            if (_player.IsSkillshotRunning && _player.SkillshotRightRampOn)
            {
                _player.IsSkillshotRunning = false;
                _player.SkillshotRightRampOn = false;
                var p = pinGod.AddPoints(_player.SkillshotRampRightAward);
                _player.SkillshotRampRightAward += MotuConstant._50K;
                pinGod.LogInfo("skillshot: right ramp awarded");
                pinGod.PlaySfx("thunder");
                PlayThenClearAnimation("rip_skillshot", "RIPPER SKILLSHOT", p.ToScoreString());
            }

            if (_player.IsRipperMultiballRunning)
            {
                ripperMball.RipperShotHit(4);
                ripperMball.RightRamp();
            }
            else
            {
                _player.CheckForRipperShot(4);                
                UpdateLamps();
            }
        }        
    }

    private void OnSpinnerHit(ulong lastChange)
    {
        _spinAudio.Play();
        pinGod.AddPoints(MotuConstant._1K);

        //todo: double check this isn't firing over and over
        if (lastChange > 1500f)
        {
            if (!_player.IsSorceressRunning)
            {
                //todo: this piece belongs in the ManAtArms mode for the input, but the last change time in that mode makes this not run because the last change is more like 3f from ManAtArms. Will need a better way like a signal
                //manat arms. 
                if (_player.IsManAtArmsRunning && !_player.ManAtArmsProgress[1])
                {
                    _player.ManAtArmsProgress[1] = true;                    
                    manAtArmsMode.CheckRunningProgressComplete(manAtArmsMode.AwardCurrentLitScore());
                }

                //beastman
                if (_player.IsBeastModeRunning && _player.BeastActiveTargets[2])
                {
                    beastModeInstance?.FoundBeastman();
                }

                //stratos
                if (_player.IsStratosRunning)
                {
                    stratosModeInstance?.AddTime();
                }

                //will check if in multiball
                if (_player.IsRipperMultiballRunning)
                {
                    ripperMball.RipperShotHit(2);
                }
                else
                {
                    _player.CheckForRipperShot(2);
                }                

                //advance grayskull if it isn't running
                if (!_player.IsGrayskullMultiballRunning && !_player.IsGraySkullReady)
                {
                    _player.AdvanceGraySkull();
                    pinGod.PlaySfx("thunder");

                    if (_player.IsGraySkullReady)
                    {
                        PlayThenClearAnimation("gray_zoom", "GRAYSKULL READY", "shoot he-man to start grayskull multiball");
                        pinGod.LogInfo("grayskull is ready from scoop");
                    }
                    else
                    {
                        PlayThenClearAnimation("gray_zoom", "GRAYSKULL ADVANCED", $"{_player.GrayskullProgress.Where(x => x).Count()} shots completed");
                        pinGod.LogInfo("grayskull advanced");
                    }
                }
                //grayskull multiball
                else if (_player.IsGrayskullMultiballRunning)
                {
                    grayskullMball.CollectJackpot();
                }

                CheckForCombo();
                UpdateLamps();
            }
        }
    }

    internal void PlayThenClearAnimation(string animation, string message, string messageSmall = "")
    {
        _anims.Animation = animation; _anims.Frame = 0;
        _anims.Play();
        _control.Visible = true;
        _label.Text = message;
        _label2.Text = messageSmall;
        _clearDisplayTimer.Start();
    }

    private void SetBallStarted()
    {
        pinGod.DisableAllLamps();
        pinGod.SolenoidOn("post", 0); //set the post to down position
        if (_clearDisplayTimer.IsStopped())
            _control.Visible = false;

        if (!pinGod.BallStarted)
        {
            pinGod.BallStarted = true;
            pinGod.PlayMusic("theme_alt");
            pinGod._trough.StartBallSaver(pinGod._trough._ball_save_seconds);
        }

        UpdateLamps();
    }

    private void StartBeastman()
    {
        pinGod.LogInfo("beastman starting");
        var game = GetParent().GetParent() as Game;
        beastModeInstance = _beastmanMode.Instance() as BeastmanMode;
        game.GetNode("Modes").AddChild(beastModeInstance);
    }

    private void StartSkeletorMultiball()
    {
        pinGod.LogInfo("starting skeletor multiball scene");        
        var game = GetParent().GetParent() as Game;
        game.GetNode("Modes").AddChild(_skeleMultiball.Instance());
    }

    private void StartStratos()
    {
        pinGod.LogInfo("stratos starting");
        var game = GetParent().GetParent() as Game;
        stratosModeInstance = _stratosMode.Instance() as StratosMode;
        game.GetNode("Modes").AddChild(stratosModeInstance);
    }
}
