/// <summary>
/// sync with external simulator. Used by memory map ReadStates
/// </summary>
public enum GameSyncState
{
    /// <summary>
    /// No state
    /// </summary>
    None,
    /// <summary>
    /// quit godot
    /// </summary>
    quit,
    /// <summary>
    /// pause godot
    /// </summary>
    pause,
    /// <summary>
    /// resume godot
    /// </summary>
    resume
}