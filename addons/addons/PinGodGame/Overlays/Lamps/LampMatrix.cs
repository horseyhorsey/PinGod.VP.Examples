using Godot;

public class LampMatrix : GridContainer
{
	[Export] int _lamp_count = 64;
	[Export] int _lamp_size_pixels = 50;

	private Lamp[] _lamps = null;
	private PackedScene _lampScene;

	public override void _EnterTree()
	{
		base._EnterTree();
		_lampScene = ResourceLoader.Load<PackedScene>("res://addons/PinGodGame/Overlays/Lamps/Lamp.tscn");
		_lamps = new Lamp[_lamp_count];
		for (int i = 0; i < _lamp_count; i++)
		{
			var lamp = _lampScene.Instance() as Lamp;
			lamp.RectMinSize = new Vector2(_lamp_size_pixels, _lamp_size_pixels);
			lamp.RectSize = new Vector2(_lamp_size_pixels, _lamp_size_pixels);
			lamp.Number = i + 1;

			_lamps[i] = lamp;
			AddChild(lamp);
		}
	}

	public void SetState(int num, int state)
	{
		if(_lamps != null)
		{
			_lamps[num - 1].SetState(state);
		}		
	}
}
