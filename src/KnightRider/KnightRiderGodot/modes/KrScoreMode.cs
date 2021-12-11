using Godot;
/// <summary>
/// Simple score display mode for 4 players with ball information. Used in the <see cref="Game"/> Scene
/// </summary>
public class KrScoreMode : ScoreMode
{	
	DashLamp _kittLamp;
	DashLamp _karrLamp;
	DashLamp _truckLamp;
	private DashLamp _videoLamp;

	/// <summary>
	/// Initialize all labels from the scene so we can update
	/// </summary>
	public override void _EnterTree()
	{	
		_kittLamp = GetNode<DashLamp>("KittPanel/HBoxContainer/DashLamp");
		_karrLamp = GetNode<DashLamp>("KittPanel/HBoxContainer/DashLamp2");
		_truckLamp = GetNode<DashLamp>("KittPanel/HBoxContainer/DashLamp3");
		_videoLamp = GetNode<DashLamp>("KittPanel/HBoxContainer/DashLamp4");

		base._EnterTree();
	}

    void UpdateLamps()
	{
		var p = pinGod.Player as KnightRiderPlayer;
		if(p != null)
		{
			//darken the lamp on screen
			if (p.KittCompleteReady) _kittLamp.LampOn(1); else { _kittLamp.LampOn(0); }
			if (p.KarrCompleteReady) _karrLamp.LampOn(1); else { _karrLamp.LampOn(0); }
			if (p.TruckCompleteReady) _truckLamp.LampOn(1); else { _truckLamp.LampOn(0); }
			if (p.IsVideoModeLit) _videoLamp.LampOn(1); else { _videoLamp.LampOn(0); }
		}
	}
}
