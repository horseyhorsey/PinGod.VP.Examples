using Godot;

/// <summary>
/// Settings for music, sfx, voice
/// </summary>
public class AudioSettings : MarginContainer
{
    private PinGodGame pinGod;
    private HSlider _volMusSlider;
    private Label _volMusLabel;
    private CheckButton _musicCheck;
    private HSlider _volSfxSlider;
    private Label _volSfxLabel;
    private CheckButton sfxCheck;
    private HSlider _volVoiceSlider;
    private Label _volVoiceLabel;
    private CheckButton _voiceCheck;
    private HSlider _volMasterSlider;
    private Label _volMasterLabel;

    public override void _EnterTree()
    {
        if (!Engine.EditorHint)
        {
            base._EnterTree();

            pinGod = GetNode<PinGodGame>("/root/PinGodGame");
            SetupMaster();
            SetupSfx();
            SetupMusic();
            SetupVoice();
        }
    }

    private void SetupMaster()
    {
        _volMasterSlider = GetNode<HSlider>("VBoxContainer/MasterContainer/HSlider");
        _volMasterSlider.Value = pinGod.GameSettings?.MasterVolume ?? 0;

        _volMasterLabel = GetNode<Label>("VBoxContainer/MasterContainer/Label");
        _volMasterLabel.Text = $"{pinGod.GameSettings?.MasterVolume}";
    }

    private void SetupVoice()
    {
        _volVoiceSlider = GetNode<HSlider>("VBoxContainer/VoiceContainer/HSlider");
        _volVoiceSlider.Value = pinGod.GameSettings.VoiceVolume;

        _volVoiceLabel = GetNode<Label>("VBoxContainer/VoiceContainer/Label");
        _volVoiceLabel.Text = $"{pinGod.GameSettings.VoiceVolume}";

        _voiceCheck = GetNode<CheckButton>("VBoxContainer/VoiceContainer/CheckButton");
        _voiceCheck.SetPressedNoSignal(pinGod.GameSettings.VoiceEnabled);
    }

    private void SetupSfx()
    {
        _volSfxSlider = GetNode<HSlider>("VBoxContainer/SfxContainer/HSlider");
        _volSfxSlider.Value = pinGod.GameSettings.SfxVolume;

        _volSfxLabel = GetNode<Label>("VBoxContainer/SfxContainer/Label");                
        _volSfxLabel.Text = $"{pinGod.GameSettings.SfxVolume}";

        sfxCheck = GetNode<CheckButton>("VBoxContainer/SfxContainer/CheckButton");
        sfxCheck.SetPressedNoSignal(pinGod.GameSettings.SfxEnabled);
    }

    private void SetupMusic()
    {
        _volMusSlider = GetNode<HSlider>("VBoxContainer/MusicContainer/HSlider");
        _volMusSlider.Value = pinGod.GameSettings.MusicVolume;

        _volMusLabel = GetNode<Label>("VBoxContainer/MusicContainer/Label");
        _volMusLabel.Text = $"{pinGod.GameSettings.MusicVolume}";

        _musicCheck = GetNode<CheckButton>("VBoxContainer/MusicContainer/CheckButton");
        _musicCheck.SetPressedNoSignal(pinGod.GameSettings.MusicEnabled);
    }

    void _on_VolumeSliderMaster_value_changed(float val)
    {
        pinGod.GameSettings.MasterVolume = val;
        _volMasterLabel.Text = val > 0 ? $"+{val}" : val.ToString();
        AudioServer.SetBusVolumeDb(0, val);
    }

    void _on_VolumeSliderMusic_value_changed(float val)
    {
        pinGod.GameSettings.MusicVolume = val;
        _volMusLabel.Text = val > 0 ? $"+{val}" : val.ToString();
        AudioServer.SetBusVolumeDb(1, val);
    }

    void _on_VolumeSliderSfx_value_changed(float val)
    {
        pinGod.GameSettings.SfxVolume = val;
        _volSfxLabel.Text = val > 0 ? $"+{val}" : val.ToString();
        AudioServer.SetBusVolumeDb(2, val);
    }

    void _on_VolumeSliderVoice_value_changed(float val)
    {
        pinGod.GameSettings.VoiceVolume = val;
        _volVoiceLabel.Text = val > 0 ? $"+{val}" : val.ToString();
        AudioServer.SetBusVolumeDb(3, val);
    }

    void _on_CheckButtonMusic_toggled(bool pressed) => pinGod.GameSettings.MusicEnabled = pressed;
    void _on_CheckButtonSfx_toggled(bool pressed) => pinGod.GameSettings.SfxEnabled = pressed;
    void _on_CheckButtonVoice_toggled(bool pressed) => pinGod.GameSettings.VoiceEnabled = pressed;
}
