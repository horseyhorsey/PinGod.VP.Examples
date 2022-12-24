using static Godot.GD;

/// <summary>
/// Logging wrapper around Godot's Print methods and PushErrors
/// </summary>
public interface IPinballLogger
{
    /// <summary>
    /// 
    /// </summary>
    PinGodLogLevel LogLevel { get; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="what"></param>
    void LogDebug(params object[] what);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="what"></param>
    void LogError(string message = null, params object[] what);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="what"></param>
    void LogInfo(params object[] what);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="what"></param>
    void LogWarning(string message = null, params object[] what);
}

/// <summary>
/// Static logger class. Prints Godot errors and push warning / errors to godot debugger
/// </summary>
public static class Logger
{
    /// <summary>
    /// 
    /// </summary>
    public static PinGodLogLevel LogLevel { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="what"></param>
    public static void LogDebug(params object[] what)
    {
        if (LogLevel <= PinGodLogLevel.Debug)
        {
            Print(what);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="what"></param>
    public static void LogError(string message = null, params object[] what)
    {
        if (LogLevel <= PinGodLogLevel.Warning)
        {
            if (what?.Length > 0) PrintErr(message, what);
            else PrintErr(message, what);
            PushError(message);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="what"></param>
    public static void LogInfo(params object[] what)
    {
        if (LogLevel <= PinGodLogLevel.Info)
        {
            Print(what);
        }
    }
    /// <summary>
    /// Logs warnings and also pushes warnings to Godot
    /// </summary>
    /// <param name="message"></param>
    /// <param name="what"></param>
    public static void LogWarning(string message = null, params object[] what)
    {
        if (LogLevel <= PinGodLogLevel.Warning)
        {
            if(what?.Length > 0) Print(message, what);
            else Print(message, what);
            PushWarning(message);
        }
    }
}