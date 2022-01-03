using Godot;
using Pingod.Local.resources;
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

        GetNode<CheckButton>("VBoxContainer/CheckButtonFullScreen").Pressed = _displaySettings.FullScreen;
        GetNode<CheckButton>("VBoxContainer/CheckButtonFullScreen").Text = ResourceText.setting_display_fs;

        GetNode<CheckButton>("VBoxContainer/CheckButtonVsync").Pressed = _displaySettings.Vsync;
        GetNode<CheckButton>("VBoxContainer/CheckButtonVsync").Text = ResourceText.setting_display_vsync;

        GetNode<CheckButton>("VBoxContainer/CheckButtonVsyncComp").Pressed = _displaySettings.VsyncViaCompositor;
        GetNode<CheckButton>("VBoxContainer/CheckButtonVsyncComp").Text = ResourceText.setting_display_vsynccomp;

        GetNode<CheckButton>("VBoxContainer/CheckButtonAlwaysOnTop").Pressed = _displaySettings.AlwaysOnTop;
        GetNode<CheckButton>("VBoxContainer/CheckButtonAlwaysOnTop").Text = ResourceText.setting_display_ontop;

        GetNode<SpinBox>("VBoxContainer/SpinBoxFPS").Value = _displaySettings.FPS;

        var stretchOption = GetNode<OptionButton>("VBoxContainer/StretchAspectOptionButton");
        stretchOption.HintTooltip = ResourceText.setting_display_aspect_ratio;
        foreach (SceneTree.StretchAspect item in Enum.GetValues(typeof(SceneTree.StretchAspect)))
        {
            stretchOption.AddItem(item.ToString(), (int)item);
        }
        stretchOption.Selected = _displaySettings.AspectOption;
    }

    void _on_CheckButtonAlwaysOnTop_toggled(bool pressed)
    {
        OS.SetWindowAlwaysOnTop(pressed);
        pinGod.GameSettings.Display.AlwaysOnTop = pressed;
    }

    void _on_CheckButtonFullScreen_toggled(bool pressed)
    {        
        CallDeferred(nameof(SetFullScreen), pressed);
    }

    void _on_CheckButtonVsync_toggled(bool pressed)
    {
        OS.VsyncEnabled = pressed;
        pinGod.GameSettings.Display.Vsync = pressed;
    }

    void _on_CheckButtonVsyncComp_toggled(bool pressed)
    {
        OS.VsyncViaCompositor = pressed;
        pinGod.GameSettings.Display.VsyncViaCompositor = pressed;
    }

    void _on_SpinBoxFPS_value_changed(float value)
    {
        Engine.TargetFps = (int)value;
        pinGod.GameSettings.Display.FPS = Engine.TargetFps;
    }

    void _on_ResetDefaultButton_button_up()
    {
        if(_displaySettings.WidthDefault > 50 && _displaySettings.HeightDefault > 50)
        {
            OS.WindowSize = new Vector2(_displaySettings.WidthDefault, _displaySettings.HeightDefault);
        }
    }

    void _on_StretchAspectOptionButton_item_selected(int index)
    {
        pinGod.GameSettings.Display.AspectOption = index;
        pinGod.SetMainSceneAspectRatio();        
    }

    private void SetFullScreen(bool pressed)
    {
        OS.WindowFullscreen = pressed;
        pinGod.GameSettings.Display.FullScreen = pressed;
    }
}
