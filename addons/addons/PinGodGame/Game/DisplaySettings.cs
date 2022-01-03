public class DisplaySettings
{
    public bool AlwaysOnTop { get; set; } = true;
    public int AspectOption { get; set; } = 0;
    public double FPS { get; set; } = 30;
    public bool FullScreen { get; set; } = false;
    public float Height { get; set; } = 600;
    public float HeightDefault { get; set; }
    public bool LowDpi { get; set; } = false;
    public bool NoWindow { get; set; }
    public bool Vsync { get; set; } = true;
    public bool VsyncViaCompositor { get; set; }
    public float Width { get; set; }
    public float WidthDefault { get; set; } = 1024;
    public float X { get; set; }
    public float Y { get; set; }    
}