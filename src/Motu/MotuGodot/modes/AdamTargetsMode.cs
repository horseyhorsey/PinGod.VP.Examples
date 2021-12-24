using Godot;

/// <summary>
/// ADAM targets: 42K for the Teela switch
/// Teela allows you to start a mode by the targets by hitting them only once.
/// In Ripper Multiball the lights do not increase but can be used with Teela.
/// Completing all ADAM targets and starting one mode is enough to light solid.
/// You can bank the Teela switches by not going for the ADAM targets.
/// Then if hit with all 3 Teela banked you start all 3.
/// </summary>
public class AdamTargetsMode : PinGodGameMode
{
    private MotuPlayer _player;
    int _doubleScoringTimeLeft = 30;
    int _bumpersTimeLeft = 30;
    int _spinnersTimeLeft = 30;
    private Timer _timer;
    private VBoxContainer _container;
    private Label _labelDs;
    private Label _labelBu;
    private Label _labelSp;
    private Label _labelTeela;
    private AnimationPlayer _animTeela;

    public override void _Ready()
    {
        base._Ready();
        _timer = GetNode<Timer>("ModeTimer");

        _container = GetNode<VBoxContainer>(nameof(VBoxContainer));
        _container.Visible = false;

        _labelDs = GetNode<Label>("VBoxContainer/Label0");
        _labelBu = GetNode<Label>("VBoxContainer/Label1");
        _labelSp = GetNode<Label>("VBoxContainer/Label2");
        ResetLabels();

        _labelTeela = GetNode<Label>("TeelaNode/HBoxContainer/Label");
        _animTeela = GetNode<AnimationPlayer>("TeelaNode/AnimationPlayer");

        pinGod.LogInfo("adam targets mode ready");
    }

    private void ResetLabels()
    {
        _labelDs.Text = "";
        _labelBu.Text = "";
        _labelSp.Text = "";
    }

    protected override void OnBallDrained()
    {
        base.OnBallDrained();
        _container.Visible = false;
        ResetLabels();
        _timer.Stop();
    }

    void ResetModeTimes()
    {
        _doubleScoringTimeLeft = 30;
        _bumpersTimeLeft = 30;
        _spinnersTimeLeft = 30;
    }

    protected override void OnBallStarted()
    {
        _player = (pinGod as MotuPinGodGame).CurrentPlayer();
        ResetModeTimes();
    }

    public void StartAdamBonusRound(MotuPlayer player, int ii)
    {
        switch (ii)
        {
            case 0:
                if (!player.IsAdamDoubleScoring)
                {
                    player.IsAdamDoubleScoring = true;
                    _doubleScoringTimeLeft = 30;
                    if (_timer.IsStopped())
                        _timer.Start();

                    _labelDs.Visible = true;
                    _container.Visible = true;
                    pinGod.LogInfo("adam: double scoring started");
                }
                break;
            case 1:
                if (!player.IsAdamSuperBumperRunning)
                {
                    player.IsAdamSuperBumperRunning = true;
                    _bumpersTimeLeft = 30;
                    if (_timer.IsStopped())
                        _timer.Start();
                    _labelBu.Visible = true;
                    _container.Visible = true;
                    pinGod.LogInfo("adam: super bumpers started");
                }
                break;
            case 2:
                if (!player.IsAdamSuperSpinnersRunning)
                {
                    player.IsAdamSuperSpinnersRunning = true;
                    _spinnersTimeLeft = 30;
                    if (_timer.IsStopped())
                        _timer.Start();
                    _labelSp.Visible = true;
                    _container.Visible = true;
                    pinGod.LogInfo("adam: super spinners started");
                }
                break;
            default:
                break;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!pinGod.GameInPlay || pinGod.IsTilted) return;
        if (pinGod.IsMultiballRunning) return;

        if(pinGod.SwitchOn("heman_lane", @event))
        {
            if(_player.TeelaCount < 3)
            {
                pinGod.AddPoints(42000);
                _player.TeelaCount++;
                DisplayTeela();
                pinGod.LogInfo("adam: heman_lane increased teela to " + _player.TeelaCount);
            }
        }
    }

    private void DisplayTeela()
    {
        _labelTeela.Text = "TEELA BANK " + _player.TeelaCount;
        _animTeela.Play("TeelaRightLeft");
    }

    void PinballTargetsControl_OnTargetsCompleted()
    {
        pinGod.LogInfo("adam: target bank completed");
    }

    void ModeTimer_timeout()
    {
        var timeSet = false;
        if (_player.IsAdamDoubleScoring)
        {
            _doubleScoringTimeLeft--;
            if (_doubleScoringTimeLeft <= 0)
            {
                _doubleScoringTimeLeft = 0;
                _player.IsAdamDoubleScoring = false;
            }
            else
                timeSet=true;
        }
        if (_player.IsAdamSuperBumperRunning)
        {
            _bumpersTimeLeft--;
            if (_bumpersTimeLeft <= 0)
            {
                _bumpersTimeLeft = 0;
                _player.IsAdamSuperBumperRunning = false;
            }
            else
                timeSet = true;
        }
        if (_player.IsAdamSuperSpinnersRunning)
        {
            _spinnersTimeLeft--;
            if (_spinnersTimeLeft <= 0)
            {
                _spinnersTimeLeft = 0;
                _player.IsAdamSuperSpinnersRunning = false;
            }
            else
                timeSet = true;
        }

        //come this far stop the timer
        if (!timeSet)
        {
            _timer.Stop();
            var game = GetParent().GetParent() as Game;
            game.StartSorceress();
        }            

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (!_player.IsAdamDoubleScoring) _labelDs.Visible = false;
        else _labelDs.Text = $"{_doubleScoringTimeLeft} - DOUBLE SCORING";

        if (!_player.IsAdamSuperBumperRunning) _labelBu.Visible = false;
        else _labelBu.Text = $"{_bumpersTimeLeft} - SUPER BUMPERS";

        if (!_player.IsAdamSuperSpinnersRunning) _labelSp.Visible = false;
        else _labelSp.Text = $"{_spinnersTimeLeft} - SUPER SPINNERS";
    }
}