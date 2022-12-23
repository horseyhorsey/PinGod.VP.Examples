using static Godot.GD;
public interface IPinballLogger
{
    PinGodLogLevel LogLevel { get; }
    void LogDebug(params object[] what);
    void LogError(string message = null, params object[] what);
    void LogInfo(params object[] what);
    void LogWarning(string message = null, params object[] what);
}

public static class Logger
{
    public static PinGodLogLevel LogLevel { get; set; }

    public static void LogDebug(params object[] what)
    {
        if (LogLevel <= PinGodLogLevel.Debug)
        {
            Print(what);
        }
    }
    public static void LogError(string message = null, params object[] what)
    {
        if (LogLevel <= PinGodLogLevel.Warning)
        {
            if (what?.Length > 0) PrintErr(message, what);
            else PrintErr(message, what);
            PushError(message);
        }
    }
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