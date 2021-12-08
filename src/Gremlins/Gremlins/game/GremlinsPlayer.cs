using System.Collections.Generic;
/// <summary>
/// Add player properties here. Rename the class and file if you wish
/// </summary>
public class GremlinsPlayer : PinGodPlayer
{
    public List<GremlinMode> CompletedModes { get; set; } = new List<GremlinMode>()
    {
        new GremlinMode{ Title = "HUNT"},
        new GremlinMode{ Title = "SPIDER"},
        new GremlinMode{ Title = "PURSUIT"},
        new GremlinMode{ Title = "MADNESS"},
    };
    public bool AllModesCompleted { get; internal set; }
    public bool CanRotateModes { get; internal set; } = true;

    public byte[] DeagleTargets { get; internal set; }
    public int DeaglesCompleted { get; internal set; }
    public GremlinMode SelectedMode { get; internal set; }
}

public class GremlinMode
{
    public string Title { get; set; }
    public bool Completed { get; set; }
    public long Score { get; set; }
}
