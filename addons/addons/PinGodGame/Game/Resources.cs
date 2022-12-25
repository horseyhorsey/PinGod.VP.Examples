using Godot;

/// <summary>
/// Resources, graphic packs. Set to AutoLoad with a Resources.tsn in ProjectSettings
/// </summary>
public class Resources : ResourcePreloader
{
    [Export] Godot.Collections.Dictionary<string, string> _resources = new Godot.Collections.Dictionary<string, string>();

    /// <summary>
    /// Working directory to load pingod.gfx.pck
    /// </summary>
    public static string WorkingDirectory = string.Empty;

    /// <summary>
    /// Looks for a pingod.gfx.pck file to load
    /// </summary>
    public override void _EnterTree()
    {
        base._EnterTree();

        if (!Engine.EditorHint)
        {           
            //when using res:// and launching the game from another directory eg: VP tables folder without game files. Try and load pack locally then if fail load from a working directory.
            var exePath = OS.GetExecutablePath();
            WorkingDirectory = System.IO.Path.GetDirectoryName(exePath);
            Logger.LogInfo("Resources WorkingDirectory: " + WorkingDirectory);

            if (!ProjectSettings.LoadResourcePack("res://pingod.gfx.pck"))
            {
                var resPath = System.IO.Path.Combine(WorkingDirectory, "pingod.gfx.pck");
                Logger.LogInfo("local gfx pck resource not found, loading from: " + resPath);
                if (!ProjectSettings.LoadResourcePack(resPath))
                {
                    Logger.LogWarning("no pingod.gfx.pck found");
                    return;
                }
            }

            LoadResources();
        }
    }
    /// <summary>
    /// Invokes GD.Load on every resource found
    /// </summary>
    private void LoadResources()
    {
        Logger.LogDebug("pre loading resources");
        foreach (var res in _resources)
        {
            var loaded = GD.Load(res.Value);
            AddResource(res.Key, loaded);
        }

        if (GetResourceList().Length > 0)
            Logger.LogDebug(string.Join(",", GetResourceList()));
        else
            Logger.LogDebug("no resources found");
    }

    /// <summary>
    /// Gets resource from the ResourcePreloader
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Resource Resolve(string name)
    {
        if (HasResource(name))
            return GetResource(name);
        else
        {
            Logger.LogWarning("resource not found. " + name);
            return null;
        }            
    }
}
