using Godot;

public class GameProgress : MarginContainer
{
	private Label barrelLabel;
	private Label bruceLabel;
	private Label orcaLabel;
	private Label doublePfLabel;

	public override void _EnterTree()
	{		
		barrelLabel = GetNode("HBoxContainer/Barrel/Label") as Label;
		bruceLabel = GetNode("HBoxContainer/Bruce/Label") as Label;
		orcaLabel = GetNode("HBoxContainer/Orca/Label") as Label;
		doublePfLabel = GetNode("HBoxContainer/2XLabel") as Label;
	}

	public void UpdateProgress(bool doublePf = false, int[] mballs = null)
	{
		if (doublePf)
			doublePfLabel.Text = "2X";
		else
			doublePfLabel.Text = string.Empty;

		if (mballs?.Length == 3)
		{
			orcaLabel.Text = mballs[0] <= 0 ? "" : mballs[0].ToString();
			barrelLabel.Text = mballs[1] <= 0 ? "" : mballs[1].ToString();
			bruceLabel.Text = mballs[2] <= 0 ? "" : mballs[2].ToString();
		}
		else
		{
			GD.PrintErr("game progress: no player found");
		}
	}
}
