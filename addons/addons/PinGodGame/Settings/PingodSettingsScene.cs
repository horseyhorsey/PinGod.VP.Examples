using Godot;
using System;

/// <summary>
/// PinGod menu settings. Like log levels and machine read write
/// </summary>
public class PingodSettingsScene : MarginContainer
{
    private PinGodGame pinGod;

    public override void _EnterTree()
    {
        base._EnterTree();
        pinGod = GetNode<PinGodGame>("/root/PinGodGame");

        var _stateDelaySpinbox = GetNode<SpinBox>("VBoxContainer/StatesDelaySpinBox");
        _stateDelaySpinbox.Value = pinGod?.GameSettings?.MachineStatesWriteDelay ?? 10;
        _stateDelaySpinbox.Prefix = Tr("SETT_STATE_DELAY");

        var _readStatesCheck = GetNode<CheckButton>("VBoxContainer/ReadStatesCheckButton");
        _readStatesCheck.Pressed = pinGod?.GameSettings?.MachineStatesRead ?? true;
        _readStatesCheck.Text = Tr("SETT_STATE_READ");

        var _writeStatsCheck = GetNode<CheckButton>("VBoxContainer/WriteStatesCheckButton");
        _writeStatsCheck.Pressed = pinGod?.GameSettings?.MachineStatesWrite ?? true;
        _writeStatsCheck.Text = Tr("SETT_STATE_WRITE");

        var logLvlSlider = GetNode<HSlider>("VBoxContainer/HBoxContainer/HSlider");
        logLvlSlider.Value = (int)pinGod.GameSettings.LogLevel;
        UpdateLoggerText();
    }

    private void UpdateLoggerText()
    {
        GetNode<Label>("VBoxContainer/HBoxContainer/Label2").Text = pinGod.GameSettings.LogLevel.ToString();
    }

    void _on_StatesDelaySpinBox_changed(int val) => pinGod.GameSettings.MachineStatesWriteDelay = val;

    void _on_ReadStatesCheckButton_toggled(bool pressed) => pinGod.GameSettings.MachineStatesRead = pressed;

    void _on_WriteStatesCheckButton_toggled(bool pressed) => pinGod.GameSettings.MachineStatesWrite = pressed;

    void _on_HSlider_value_changed(float val)
    {
        var lvl = (PinGodLogLevel)val;
        pinGod.GameSettings.LogLevel = lvl;
        UpdateLoggerText();
    }
}
