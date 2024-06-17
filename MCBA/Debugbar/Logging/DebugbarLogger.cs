namespace MCBA.Debugbar.Logging;

public class DebugbarLogger : ILogger
{

    public static Dictionary<string, string> colors;

    public DebugbarLogger()
    {
        colors = new Dictionary<string, string>();
        colors.Add("Information","greenyellow");
        colors.Add("Debug","cornflowerblue");
        colors.Add("Warning","#ffae42");
        colors.Add("Error","#ff4040");
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (state.ToString().Contains("DbCommand"))
        {
            Kernel.Instance().AddQuery(logLevel.ToString(),state.ToString());
        }
        else
        {
            Debugbar.Message(logLevel.ToString(),state.ToString());
        }

        if (exception != null)
        {
            Kernel.Instance().AddException(exception);
        }
    }
}