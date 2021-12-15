using Godot;

public class HurryUp : Control
{
    int hurryUpTotal;
    private Label label;
    private Timer timer;
    private PinGodGame pinGod;
    private NightmarePlayer _player;
    private Game _game;

    public override void _Ready()
    {
        base._Ready();

        label = GetNode<Label>("NightmareMessage");
        timer = GetNode<Timer>("Timer");
        pinGod = GetNode<PinGodGame>("/root/PinGodGame");
        _player = pinGod.Player as NightmarePlayer;
        _game = GetParent().GetParent() as Game;
    }

    public void HurryUpTimer_timeout()
    {
        hurryUpTotal -= 250000;
        if (hurryUpTotal <= 0)
        {
            this.Visible = false;
            timer.Stop();
        }
        else
        {
            UpdateText();
        }
    }

    public void StartHurryUp()
    {
        pinGod.LogInfo("hurry up: starting");
        _player.HurryUpRunning = true;
        hurryUpTotal = 20000000;
        this.Visible = true;
        timer.Start();
    }

    void OnBallDrained()
    {
        pinGod.LogInfo("drained: stopping hurry up");
        timer.Stop();
        _player.HurryUpRunning = false;
        this.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if(pinGod.GameInPlay && !pinGod.IsTilted)
        {
            if (_player?.HurryUpRunning ?? false)
            {
                if (pinGod.SwitchOn("orbit_r", @event))
                {
                    timer.Stop();
                    _player.HurryUpRunning = false;
                    pinGod.AddPoints(hurryUpTotal);
                    this.Visible = false;
                    //_game.PlayThenResumeMusic("mus_hurryupcollected", 3.9f);
                    _game.OnDisplayMessage($"HURRY UP\n{hurryUpTotal.ToScoreString()}");
                    pinGod.LogInfo("hurry up collected: " + hurryUpTotal);
                }
            }
        }        
    }

    void UpdateText() => label.Text = $"HURRY UP\n{hurryUpTotal.ToScoreString()}";
}