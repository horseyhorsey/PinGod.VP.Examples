using Godot;

/// <summary>
/// A Basic text layer with optional blinking animations. Use export properties from the godot editor. <para/>
/// TextLayer.tscn
/// </summary>
public class TextLayer : Control
{
	[Export]
	private string _text;
	[Export]
	private float _blinking;

	[Export(PropertyHint.ColorNoAlpha, "ffffff")]
	private string _font_color = "ff0000ff";

	[Export(PropertyHint.ColorNoAlpha, "000000ff")]
	private string _stroke_color = "000000ff";

	private Label textLabel;
	private AnimationPlayer animationPlayer;

	/// <summary>
	/// Gets label and player from the scene, sets colour and blink speed
	/// </summary>
	public override void _Ready()
	{
		textLabel = this.GetNode("CenterContainer/Label") as Label;
		animationPlayer = this.GetNode("AnimationPlayer") as AnimationPlayer;

		//setup font colours, stroke
		if(textLabel != null)
		{
			textLabel.AddColorOverride("font_color", new Color(_font_color));
			textLabel.AddColorOverride("font_outline_modulate", new Color(_stroke_color));
		}

		SetText(_text);

		if (_blinking > 0)
		{
			animationPlayer.PlaybackSpeed = _blinking;
			animationPlayer.Play("BlinkText");
		}
	}

	public void SetText(string text)
	{
		textLabel.Text = text;
	}
}
