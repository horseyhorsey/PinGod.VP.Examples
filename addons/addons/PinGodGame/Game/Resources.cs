using Godot;

public class Resources : ResourcePreloader
{
    [Export] Godot.Collections.Dictionary<string, string> _resources = new Godot.Collections.Dictionary<string, string>();

    public override void _EnterTree()
    {
        base._EnterTree();

        if (!Engine.EditorHint)
        {
            Logger.LogInfo($"gfx pack loaded...{ProjectSettings.LoadResourcePack("res://pingod.gfx.pck")}");
            
            LoadResources();
        }
    }

    private void LoadResources()
    {
        Logger.LogInfo("pre loading resources");
        foreach (var res in _resources)
        {
            var loaded = GD.Load(res.Value);
            AddResource(res.Key, loaded);
        }

        if (GetResourceList().Length > 0)
            Logger.LogInfo(string.Join(",", GetResourceList()));
        else
            Logger.LogInfo("no resources found");
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
