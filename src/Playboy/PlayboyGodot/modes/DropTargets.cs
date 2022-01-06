public class DropTargets : PinGodGameMode
{
	private PlayboyPlayer _player;
    private PinballTargetsBank _targets;

    public override void _EnterTree()
	{
		base._EnterTree();
		_targets = GetNode<PinballTargetsBank>(nameof(PinballTargetsBank));
	}

    protected override void OnBallStarted()
    {
        _player = pinGod.Player as PlayboyPlayer;
        _targets.ResetTargets();
        pinGod.SolenoidPulse("drop_bank");
    }

    protected override void UpdateLamps()
    {
        base.UpdateLamps();
    }

    void _on_PinballTargetsBank_OnTargetActivated(string swName, bool complete)
    {
        pinGod.AddPoints(Game.MINSCORE);
        pinGod.AddBonus(Game.MINBONUS);
        pinGod.PlaySfx("horse-disco-pop");
    }

    void _on_PinballTargetsBank_OnTargetsCompleted()
    {
		if (_player != null)
		{
			pinGod.PlaySfx("horse-disco-laser");
			_player.DropsCompleted++;

			if (_player.DropsCompleted == 1)
			{
				pinGod.AddPoints(25000);
			}

			else if (_player.DropsCompleted >= 2)
			{
				pinGod.AddPoints(50000);
			}

			_player.OutlanesLit = true;
			_player.SpecialRightLit = true;

			pinGod.SolenoidPulse("drop_bank");
		}
	}
}
