public class DropTargets : PinballTargetsControl
{
	private PlayboyPlayer _player;

	public override void _EnterTree()
	{
		base._EnterTree();		
	}

	public override bool SetTargetComplete(int index)
	{
		bool result = base.SetTargetComplete(index);
		pinGod.AddPoints(Game.MINSCORE);
		pinGod.AddBonus(Game.MINBONUS);
		pinGod.PlaySfx("horse-disco-pop");
		return result;
	}

	public override void TargetsCompleted(bool reset = true)
	{
		base.TargetsCompleted(reset);

		if(_player != null)
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

	void OnBallStarted()
	{
		_player = pinGod.Player as PlayboyPlayer;
		ResetTargets();
		pinGod.SolenoidPulse("drop_bank");
	}

	public override void UpdateLamps()
	{
		base.UpdateLamps();
	}
}
