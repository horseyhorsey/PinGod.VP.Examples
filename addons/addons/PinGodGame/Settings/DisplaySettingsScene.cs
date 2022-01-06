using Godot;
using System;

public class DisplaySettingsScene : MarginContainer
{
    private DisplaySettings _displaySettings;
    private PinGodGame pinGod;
    public override void _EnterTree()
    {
        base._EnterTree();

        pinGod = GetNode<PinGodGame>("/root/PinGodGame");
        _displaySettings = pinGod.GameSettings.Display;
    }

    public override void _Ready()
    {
        base._Ready();

        GetNode<Label>("VBoxContainer/HBoxContainer/DefaultWindowSizeLabel").Text = $"ORIGINAL RESOLUTION: {_displaySettings.WidthDefault} X {_displaySettings.HeightDefault}";

        GetNode<CheckButton>("VBoxContainer/CheckButtonFullScreen").Pressed = (bool)ProjectSettings.GetSetting("display/window/size/fullscreen");
        GetNode<CheckButton>("VBoxContainer/CheckButtonVsync").Pressed = (bool)ProjectSettings.GetSetting("display/window/vsync/use_vsync");
        GetNode<CheckButton>("VBoxContainer/CheckButtonVsyncComp").Pressed = (bool)ProjectSettings.GetSetting("display/window/vsync/vsync_via_compositor");
        GetNode<CheckButton>("VBoxContainer/CheckButtonAlwaysOnTop").Pressed = (bool)ProjectSettings.GetSetting("display/window/size/always_on_top");

        //force fps debug?
        var fpsStr = ProjectSettings.GetSetting("debug/settings/fps/force_fps").ToString();
        int.TryParse(fpsStr, out var fps);
        GetNode<SpinBox>("VBoxContainer/SpinBoxFPS").Value = fps;        

        var stretchOption = GetNode<OptionButton>("VBoxContainer/StretchAspectOptionButton");
        foreach (SceneTree.StretchAspect item in Enum.GetValues(typeof(SceneTree.StretchAspect)))
        {
            stretchOption.AddItem(item.ToString(), (int)item);
        }
        var val = ProjectSettings.GetSetting("display/window/stretch/aspect").ToString();
        PinGodStretchAspect aspect = (PinGodStretchAspect)Enum.Parse(typeof(PinGodStretchAspect), val);
        stretchOption.Selected = (int)aspect;
    }

    void _on_CheckButtonAlwaysOnTop_toggled(bool pressed)
    {
        OS.SetWindowAlwaysOnTop(pressed);
        ProjectSettings.SetSetting("display/window/size/always_on_top", pressed);
    }

    void _on_CheckButtonFullScreen_toggled(bool pressed)
    {        
        CallDeferred(nameof(SetFullScreen), pressed);
    }

    void _on_CheckButtonVsync_toggled(bool pressed)
    {
        OS.VsyncEnabled = pressed;
        ProjectSettings.SetSetting("display/window/vsync/use_vsync", pressed);
    }

    void _on_CheckButtonVsyncComp_toggled(bool pressed)
    {
        OS.VsyncViaCompositor = pressed;
        ProjectSettings.SetSetting("display/window/vsync/vsync_via_compositor", pressed);        
    }

    void _on_SpinBoxFPS_value_changed(float value)
    {
        Engine.TargetFps = (int)value;
        ProjectSettings.SetSetting("debug/settings/fps/force_fps", Engine.TargetFps);
    }

    void _on_ResetDefaultButton_button_up()
    {
        if(_displaySettings.WidthDefault > 50 && _displaySettings.HeightDefault > 50)
        {
            OS.WindowSize = new Vector2(_displaySettings.WidthDefault, _displaySettings.HeightDefault);                 
            //don't save the project settings width / height as this will override in the settings. when changed here it will add it into the override.cfg
        }
    }

    void _on_StretchAspectOptionButton_item_selected(int index)
    {         
        ProjectSettings.SetSetting("display/window/stretch/aspect", ((PinGodStretchAspect)index).ToString());
        pinGod.SetMainSceneAspectRatio();
    }

    private void SetFullScreen(bool pressed)
    {
        OS.WindowFullscreen = pressed;
        ProjectSettings.SetSetting("display/window/size/fullscreen", pressed);
    }
}
