using Godot;
using MoonStation.game;

public class SongOptionButton : OptionButton
{
    private MsPinGodGame pinGod;
    private MsGameSettings settings;

    public override void _EnterTree()
    {
        base._EnterTree();

        this.AddItem("off", 0);
        this.AddItem("techno", 1);
        this.AddItem("dnb", 2);

        if (!Engine.EditorHint)
        {
            pinGod = GetNode<PinGodGame>("/root/PinGodGame") as MsPinGodGame;
            settings = pinGod.GameSettings as MsGameSettings;
            if (settings?.Music != "off")
                Selected = 0;
            else
            {
                if (settings?.Music == "dnb")
                    Selected = 2;
                else
                    Selected = 1;
            }
        }
    }


    void _on_OptionButton_item_selected(int index)
    {
        if (!Engine.EditorHint)
        {
            if (index > 0)
            {
                string music = GetItemText(index);
                pinGod.LogInfo($"selected {index} " + music);
                pinGod?.SetMusicOn(music);
            }                
            else
                pinGod?.SetMusicOff();
        }
    }
}
