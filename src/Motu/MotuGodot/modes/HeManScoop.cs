using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Scoop mode always running
/// </summary>
public class HeManScoop : PinGodGameMode
{
    [Export] int _modeTime;
    [Export] Godot.Collections.Array<AudioStream> _streams;

    private BallStackPinball _ballStack;

    public MotuPinGodGame MotuPinGodGame { get; private set; }

    private AnimatedSprite _anims;
    private Label _label;
    private Label _label2;
    private Control _control;
    private PackedScene _graySkullMultiball;
    private PackedScene _ripperMultiball;
    private PackedScene _sorceressMode;
    private PackedScene _motuMultiball;
    private MotuPlayer _player;

    public override void _EnterTree()
    {
        base._EnterTree();
        _ballStack = GetNode<BallStackPinball>("BallStackPinball");
        MotuPinGodGame = pinGod as MotuPinGodGame;
        _anims = GetNode<AnimatedSprite>("Control/Anims");
        _label = GetNode<Label>("Control/Label");
        _label2 = GetNode<Label>("Control/Label2");
        _control = GetNode<Control>("Control");

        var mainScene = GetNodeOrNull<MotuMainScene>("/root/MainScene");
        if(mainScene == null) //for testing when main scene isn't running
        {
            pinGod.LogWarning("main scene not found");
            _graySkullMultiball = ResourceLoader.Load("res://modes/MultiballGrayskull.tscn") as PackedScene;
            _ripperMultiball = ResourceLoader.Load("res://modes/MultiballRipper.tscn") as PackedScene;
            _sorceressMode = ResourceLoader.Load("res://modes/SorceressMode.tscn") as PackedScene;
            _motuMultiball = ResourceLoader.Load("res://modes/MultiballMotu.tscn") as PackedScene;
        }
        else //get the packed scenes from main
        {
            _graySkullMultiball = mainScene._graySkullMultiball;
            _ripperMultiball = mainScene._ripperMultiball;
            _sorceressMode = mainScene._sorceressMode;
            _motuMultiball = mainScene._motuMultiball;
        }

        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        _baseMode = GetParent().GetNode<BaseMode>("BaseMode");        
    }

    public override void _Ready()
    {
        base._Ready();

        pinGod.LogInfo("heman scoop ready");
        _control.Visible = false; //hide scoop layers
    }

    protected override void OnBallDrained()
    {
        pinGod.LogInfo("heman ball drained");
    }

    protected override void OnBallStarted()
    {
        _player = (pinGod as MotuPinGodGame).CurrentPlayer();
    }

    Dictionary<string, bool> _modesToStart;
    private AudioStreamPlayer _audio;
    private BaseMode _baseMode;
    private bool _manatarmsStarting;

    private void BallStackPinball_SwitchActive()
    {
        pinGod.LogInfo("heman scoop active");
        pinGod.SolenoidOn("heman_toy", 1);

        if (!pinGod.GameInPlay || pinGod.IsTilted)
        {
            _ballStack.Start(1f);
            return;
        }

        if (_player.IsMotuMultiballRunning)
        {
            _ballStack.Start(1.2f);
            return;
        }

        //kick ball if sorc running
        if (_player.IsSorceressRunning)
        {
            _ballStack.Start(1.5f); //kick ball
            return;
        }

        if (_player.IsWizardReady && !pinGod.IsMultiballRunning)
        {
            pinGod.LogInfo("scoop: wizard ready");

            if (_player.IsSorceressReady())
            {
                pinGod.DisableAllLamps();
                CallDeferred(nameof(StartSorceress));
                return;
            }            
            else if (_player.IsSorceressComplete && !_player.IsMotuMultiballRunning)
            {
                pinGod.LogInfo("starting motu multiball");
                CallDeferred(nameof(StartMotuMultiball));                
                return;
            }
            else
            {
                pinGod.LogInfo("wizard nothing happened..");
                _ballStack.Start(1f);
            }            
        }

        
        //clear modes to start 
        ResetModesToComplete();

        if (_player.IsMysteryReady && !_player.IsAnyMultiballRunning())
        {
            pinGod.LogInfo("mystery collected");
            (pinGod as MotuPinGodGame).AddPoints(MotuConstant._50K);
            _player.IsMysteryReady = false;
            _modesToStart["mystery"] = true;
        }

        if (!_player.IsAnyMultiballRunning())
        {
            pinGod.PlaySfx("spin_sword");
        }

        if (!_player.IsManAtArmsRunning && _player.IsManAtArmsReady && !_manatarmsStarting)
        {
            _modesToStart["manatarms_start"] = true;
            _player.SetLadder("manatarms", MotuLadder.PartComplete);
            _player.IsManAtArmsReady = false;
            _manatarmsStarting = true;
            pinGod.LogInfo("manatarms starting from scoop");
        }
        else if (_player.IsManAtArmsRunning)
        {
            pinGod.LogInfo("man runnning base: heman scoop");

            if (!_player.ManAtArmsProgress[3])
                _modesToStart["manatarms"] = true;

        }

        // add multiballs if ready and skele isn't running
        if (!_player.IsSkeleMultiballEnding || !_player.IsSkeleMultiballRunning)
        {
            //add grayskull?
            if (!_player.IsGrayskullMultiballRunning && !_player.GrayskullProgress.Any(x => !x))
            {
                _modesToStart["grayskullmball"] = true;
            }

            //add ripper?
            if (_player.IsRipperMultiballReady && !_player.IsRipperMultiballRunning)
            {
                pinGod.LogInfo("ripper multiball started from scoop");
                _player.IsRipperMultiballReady = false;
                _modesToStart["rippermball"] = true;
            }
        }

        pinGod.LogInfo("scoop starting modes");
        if (_player.IsComboAvailable && _player.BankedCombos > 0)
        {
            _modesToStart["combo"] = true;             
        }

        if (_player.IsSkillshotRunning)
        {
            _baseMode.StopLaunchBallTimer();
            pinGod.BallStarted = true;
            _player.IsSkillshotRunning = false;
            _player.SkillshotAwarded = true;
            pinGod.AddPoints(_player.SkillshotScoopAward);
            _player.SkillshotScoopAward += MotuConstant._50K;
            pinGod.LogInfo("skillshot: scoop awarded");
            _modesToStart["skillshot"] = true;
        }

        _ballStack.Start(1f);

    }

    private void ResetModesToComplete()
    {
        _modesToStart = new Dictionary<string, bool>()
        {
            { "skillshot" , false }, { "combo" , false }, { "manatarms" , false }, { "mystery" , false },
            { "rippermball" , false }, { "grayskullmball" , false },{ "manatarms_start" , false }
        };
    }

    public void BallStackPinball_SwitchInActive()
    {
        pinGod.LogInfo("heman scoop inactive");
        if (_manatarmsStarting)
        {
            _player.IsManAtArmsRunning = true;
            _manatarmsStarting = false;
            GetParent().GetNodeOrNull<ManAtArms>("ManAtArms")?._modeTimer.Start();
        }
    }

    public void BallStackPinball_timeout()
    {
        pinGod.SolenoidOn("vpcoil", 0);

        if (_modesToStart?.Any(x => x.Value) ?? false)
        {
            StartNextAward();
            return;
        }

        pinGod.SolenoidOn("heman_toy", 0);
        _ballStack.SolenoidPulse();
        _label.Text = "";

        if (!pinGod.IsMultiballRunning)
        {
            PlayModeSequence("exit"); //heman roll seq
            _control.Visible = true;
        }
        else
        {
            _control.Visible = false;
        }

        if (!_player.IsWizardReady)
        {            
            var game = GetParent().GetParent() as Game;
            pinGod.UpdateLamps(game.GetTree());
        }        
    }

    private void StartNextAward()
    {
        if(!pinGod.IsMultiballRunning)
            pinGod.SolenoidOn("vpcoil", 6); //scoop show

        float delay = 2f;
        foreach (var awardMode in _modesToStart)
        {
            if (awardMode.Value)
            {
                _modesToStart[awardMode.Key] = false;
                pinGod.LogInfo(awardMode.Key, " scoop starting");
                delay = PlayModeSequence(awardMode.Key);
                _control.Visible = true;
                break;
            }
        }

        _ballStack.Start(delay);
    }

    void PlaySequence(string anim, string msg, string msg2)
    {
        _anims.Animation = anim;
        _anims.Frame = 0;
        _anims.Play();
        _label.Text = msg;
        _label2.Text = msg2;
    }

    private float PlayModeSequence(string key)
    {
        _label2.Text = string.Empty;
        switch (key)
        {
            case "combo":
                var award = MotuPinGodGame.AwardCombo().ToScoreString();
                _baseMode._comboTimer.Stop();
                pinGod.SolenoidOn("vpcoil", 8); //PowerCombo lamps
                if (!_player.IsAnyMultiballRunning())
                {                                        
                    _audio.Stream = _streams[1];
                    _audio.Play(); //play by the power                    
                    PlaySequence(key, "POWER COMBO", $"X {_player.BankedCombos + 1} = " + award);
                    return 2.4f;
                }
                return 0f;
            case "mystery":
                if (!_player.IsAnyMultiballRunning())
                {
                    AwardMystery(key);
                    _anims.Frame = 0;
                    _anims.Play();
                    return 2f;
                }
                return 0f;
            case "skillshot":                
                PlaySequence("skillshot", "SKILLSHOT", $"SCOOP {_player.SkillshotScoopAward.ToScoreString()}");
                return 1.5f;
            case "grayskullmball":
                StartGraySkullMultiball();
                PlaySequence("grayskull", "GRAYSKULL MULTIBALL", "ROTON TARGETS ADD A BALL");
                if (!_player.IsAnyMultiballRunning())
                {                    
                    return 2f;
                }
                else return 0f;
            case "manatarms":
                if (!_player.IsAnyMultiballRunning())
                {
                    PlaySequence("man_talk", "MAN AT ARMS", "DUNCAN SHOT COMPLETED");
                    return 2f;
                }
                else return 0f;
            case "manatarms_start":
                if (!_player.IsAnyMultiballRunning())
                {
                    _audio.Stream = _streams[0];
                    _audio.Play(); //play man notime to lose
                    PlaySequence("man_talk", "MAN AT ARMS", "SHOOT BLINKING MAN SHOTS TO COMPLETE");
                    return 2f;
                }
                else return 0f;
            case "rippermball":
                StartRipperMultiball();
                if (!_player.IsAnyMultiballRunning())
                {                  
                    PlaySequence("ripper", "RIPPER MULTIBALL", "DOUBLE SUPER JACKPOT COMPLETES MODE");
                    return 2f;
                }                
                else return 0f;
            case "exit":
                _anims.Animation = "roll";
                _anims.Frame = 0;
                _anims.Play();
                return 1f;
            default:
                return 2f;
        }
    }

    private void AwardMystery(string animation)
    {
        _anims.Animation = animation;
        _label.Text = "MYSTERY";
        var mysteryAwards = new List<string>((pinGod as MotuPinGodGame).mystery_awards);
        if (_player.IsRotonReady) mysteryAwards.Remove("Adv Roton");
        if (!_player.IsManAtArmsRunning) mysteryAwards.Remove("Adv Man");
        if (_player.IsStratosRunning) mysteryAwards.Remove("Start Stratos");
        if (_player.IsGrayskullMultiballRunning || _player.IsSkeleMultiballRunning)
        {
            mysteryAwards.Remove("Start GraySkull");
        }
        if (_player.IsGraySkullReady || _player.IsGrayskullMultiballRunning)
            mysteryAwards.Remove("Adv GraySkull");

        var rand = new Godot.RandomNumberGenerator().RandiRange(0, mysteryAwards.Count - 1);
        var award = mysteryAwards[rand];
        switch (award)
        {
            case "Adv Roton":
                _baseMode.AdvanceRoton(0);
                break;
            case "Adv GraySkull":
                _player.AdvanceGraySkull();
                break;
            case "Adv Man":
                for (int i = 0; i < _player.ManAtArmsProgress.Length; i++)
                {
                    if (!_player.ManAtArmsProgress[i])
                    {
                        _player.ManAtArmsProgress[i] = true;
                        break;
                    }
                }
                break;
            case "Adv Stratos":
                _player.AdvanceStratos();
                break;
            case "500K":
                pinGod.AddPoints(MotuConstant._500K);
                break;
            case "Lite Extra Ball":
                _player.ExtraBallsAwarded++;
                break;
            case "Lite Special":
                _player.IsSpecialLit = true;
                break;
            case "Start GraySkull":
                CallDeferred(nameof(StartGraySkullMultiball));
                break;
            default:
                break;
        }
        _label2.Text = award + " AWARDED";
    }

    private void StartRipperMultiball()
    {
        pinGod.LogInfo("loading ripper multiball scene");
        var game = GetParent().GetParent() as Game;
        var modes = game.GetNode("Modes") as Node;
        modes.GetTree().CallGroup("multiball", "ResetMultiball");
        _baseMode.ripperMball = _ripperMultiball.Instance() as MultiballRipper;        
        modes.AddChild(_baseMode.ripperMball);
    }

    void Anims_animation_finished()
    {
        if (_anims.Animation == "roll")
        {
            _control.Visible = false;
        }            
    }

    private void StartGraySkullMultiball()
    {
        pinGod.LogInfo("loading grayskull multiball scene");
        var game = GetParent().GetParent() as Game;
        var modes = game.GetNode("Modes") as Node;
        modes.GetTree().CallGroup("multiball", "ResetMultiball");
        _baseMode.grayskullMball = _graySkullMultiball.Instance() as MultiballGrayskull;        
        modes.AddChild(_baseMode.grayskullMball);
    }

    private void StartSorceress()
    {
        pinGod.LogInfo("loading sorceress scene");
        var game = GetParent().GetParent() as Game;
        var modes = game.GetNode("Modes") as Node;
        modes.AddChild(_sorceressMode.Instance());
    }

    private void StartMotuMultiball()
    {
        pinGod.LogInfo("loading motu multiball scene");
        var game = GetParent().GetParent() as Game;
        var modes = game.GetNode("Modes");
        modes.AddChild(_motuMultiball.Instance());
    }
}