using Godot;

public class LampMatrix : GridContainer
{
	[Export] int _lamp_count = 64;
	[Export] Vector2 _lamp_size_pixels = new Vector2(50, 50);

	private Godot.Collections.Array<Lamp> _lamps = null;
	private PackedScene _lampScene;

	public override void _EnterTree()
	{
		base._EnterTree();
		_lampScene = ResourceLoader.Load<PackedScene>("res://addons/PinGodGame/Overlays/Lamps/Lamp.tscn");
		_lamps = new Godot.Collections.Array<Lamp>();
		for (int i = 0; i < _lamp_count; i++)
		{
			var lamp = _lampScene.Instance() as Lamp;
			lamp.RectMinSize = _lamp_size_pixels;
			lamp.RectSize = _lamp_size_pixels;
			lamp.Number = i + 1;

			_lamps.Add(lamp);
			AddChild(lamp);
		}
	}

	public void SetLabel(int num, string label)
	{
		if (_lamps?.Count > 0)
		{
			_lamps[num - 1].SetLabel(label);
		}
	}

	public void SetState(int num, int state)
	{
		if(_lamps?.Count > 0)
		{
			_lamps[num - 1].SetState(state);
		}		
	}
}
