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
    private AnimationPlayer _animTeela;
    int _bumpersTimeLeft = 30;
    private VBoxContainer _container;
    int _doubleScoringTimeLeft = 30;
    private MotuPinGodGame _game;
    private Label _labelBu;
    private Label _labelDs;
    private Label _labelSp;
    private Label _labelTeela;
    private MotuPlayer _player;
    int _spinnersTimeLeft = 30;
    private PinballTargetsBank _targetBank;
    private Timer _timer;

    public override void _EnterTree()
    {
        base._EnterTree();

        _game = (pinGod as MotuPinGodGame);
        _targetBank = GetNode<PinballTargetsBank>(nameof(PinballTargetsBank));
    }

    public override void _Input(InputEvent @event)
    {
        if (!pinGod.GameInPlay || pinGod.IsTilted) return;
        if (pinGod.IsMultiballRunning) return;

        if (pinGod.SwitchOn("heman_lane", @event))
        {
            if (_player.TeelaCount < 3)
            {
                pinGod.AddPoints(42000);
                _player.TeelaCount++;
                DisplayTeela();
                pinGod.LogInfo("adam: heman_lane increased teela to " + _player.TeelaCount);
            }
        }
    }

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

    protected override void OnBallDrained()
    {
        base.OnBallDrained();
        _container.Visible = false;
        ResetLabels();
        _timer.Stop();
        _player.AdamTargets = _targetBank._targetValues;
        pinGod.LogInfo("adam saved player targets");
    }

    protected override void OnBallStarted()
    {
        _player = (pinGod as MotuPinGodGame).CurrentPlayer();
        _targetBank._targetValues = _player.AdamTargets;
        ResetModeTimes();
    }

    protected override void UpdateLamps()
    {
        base.UpdateLamps();
        var player = _game.CurrentPlayer();
        //adam target bonuses
        if (player.IsAdamDoubleScoring) pinGod.SetLampState("adam_double", 2);
        else pinGod.SetLampState("adam_double", 0);
        if (player.IsAdamSuperBumperRunning) pinGod.SetLampState("adam_bump", 2);
        else pinGod.SetLampState("adam_bump", 0);
        if (player.IsAdamSuperSpinnersRunning) pinGod.SetLampState("adam_spin", 2);
        else pinGod.SetLampState("adam_spin", 0);
    }

    void _on_PinballTargetsBank_OnTargetActivated(string swName, bool complete)
    {
        if (!_player.IsRipperMultiballRunning && !_player.IsSorceressRunning)
        {
            if (_player.TeelaCount >= 1)
            {
                AddAdamTargetsBonusRound(_player);
                _player.TeelaCount = 0;
            }

            if(complete)
                _player.SetLadder("adam", MotuLadder.PartComplete);
        }
    }

    void _on_PinballTargetsBank_OnTargetsCompleted()
    {
        AddAdamTargetsBonusRound(_game.CurrentPlayer(), 1);
    }

    private void AddAdamTargetsBonusRound(MotuPlayer player, int numOfModesToStart = 1)
    {
        //todo: play seq AdamTargets
        player.SetLadder("adam", MotuLadder.Complete);
        pinGod.LogInfo($"starting ({numOfModesToStart}) adam bonus rounds");
        for (int i = 0; i < numOfModesToStart; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                if (!player.AdamModesRunning[ii])
                {
                    player.AdamModesRunning[ii] = true;
                    StartAdamBonusRound(player, ii);
                    break;
                }
            }
        }
    }

    private void DisplayTeela()
    {
        _labelTeela.Text = "TEELA BANK " + _player.TeelaCount;
        _animTeela.Play("TeelaRightLeft");
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
                timeSet = true;
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

    void PinballTargetsControl_OnTargetsCompleted()
    {
        pinGod.LogInfo("adam: target bank completed");
    }

    private void ResetLabels()
    {
        _labelDs.Text = "";
        _labelBu.Text = "";
        _labelSp.Text = "";
    }

    void ResetModeTimes()
    {
        _doubleScoringTimeLeft = 30;
        _bumpersTimeLeft = 30;
        _spinnersTimeLeft = 30;
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