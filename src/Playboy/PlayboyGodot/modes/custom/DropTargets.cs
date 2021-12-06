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
		return result;
	}

	public override void TargetsCompleted(bool reset = true)
	{
		base.TargetsCompleted(reset);
		//todo: watch what game is doing when the drop targets are completed
		if(_player != null)
		{
			_player.DropsCompleted++;
			if (_player.DropsCompleted == 1) _player.OutlanesLit = true;
			else if (_player.DropsCompleted == 2) {; }//todo: award special

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
