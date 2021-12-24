using Godot;

/// <summary>
/// Cycles red arrow shots. Shots completed find beastman
/// </summary>
public class BeastmanMode : PinGodGameMode
{
    private AnimatedSprite _beastAnim;

    string[] _beastLamps = new string[]
        {
        "arrow_l_left_loop", "arrow_l_left_ramp", "arrow_l_inner_spin",
        "arrow_l_inner_loop", "arrow_l_right_ramp", "arrow_l_right_loop"
    };

    [Export] AudioStream _foundSound;
    private MotuPlayer _player;
    bool firstShot = true;

    public override void _ExitTree()
    {
        base._ExitTree();
        _player.IsBeastModeRunning = false;

        var game = GetParent().GetParent() as Game;
        game.StartSorceress();
    }

    public override void _Ready()
    {
        base._Ready();
        pinGod.LogInfo("beastman ready");

        //get the player and setup beastman round
        _player = ((MotuPinGodGame)pinGod)?.CurrentPlayer() ?? new MotuPlayer();

        //set up beastman
        _player.StartBeastman();

        _beastAnim = GetNode<AnimatedSprite>("Control/BeastAnim");

        //don't show the scene if a multiball is running for the player
        if (!pinGod.IsMultiballRunning || !_player.IsAnyMultiballRunning())
        {            
            _beastAnim.Animation = "walk";
            _beastAnim.Play();
            GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play();
        }
        else
        {
            SetControlVisible(false);
        }        
    }

    protected override void OnBallDrained()
    {
        pinGod.LogDebug("beastman drained, removing mode");
        this.QueueFree();
    }

    /// <summary>
    /// Sets the Red arrow to flashing if beast target is ready
    /// </summary>
    protected override void UpdateLamps()
    {
        for (int i = 0; i < _player.BeastActiveTargets.Length; i++)
        {
            pinGod.SetLampState(_beastLamps[i], (byte)(_player.BeastActiveTargets[i] ? 2 : 0));
        }
    }

    /// <summary>
    /// Collect beastman award and remove layer on timer
    /// </summary>
    internal void FoundBeastman()
    {
        //turn off the switch handlers
        this.SetProcessInput(false);

        _player.SetBeastmanFound();        
        pinGod.AddPoints(_player.BeastManScore);
        
        pinGod.LogInfo("beastman: found, award at " + _player.BeastManScore);
        
        //don't show the scene if a multiball is running for the player
        if(!pinGod.IsMultiballRunning || !_player.IsAnyMultiballRunning())
        {
            SetControlVisible(true);
            _beastAnim.Animation = "blink";
            _beastAnim.Play();
            GetNode<Label>("Control/Label").Text = $"BEASTMAN FOUND";
            GetNode<Label>("Control/Label2").Text = $"{_player.BeastManScore.ToScoreString()}";
        }        

        var audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        audio.Stream = _foundSound;
        audio.Play();

        GetNode<Timer>("RemoveModeTimer").Start();
    }

    void RemoveModeTimer_timeout()
    {
        this.QueueFree();
    }

    /// <summary>
    /// Hides the animations
    /// </summary>
    /// <param name="visible"></param>
    void SetControlVisible(bool visible = false) => GetNode<Control>("Control").Visible = visible;

    /// <summary>
    /// Changes shot target on timer
    /// </summary>
    void ShotChangeTimer_timeout()
    {
        if (firstShot)
        {
            firstShot = false;
            SetControlVisible();
            _beastAnim.Stop();
        }
        _player.ChangeActiveBeastTarget();
        UpdateLamps();
    }
}