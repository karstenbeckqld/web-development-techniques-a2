using System.Collections.Concurrent;

namespace MCBA.Debugbar.Logging;

public class DebugbarLoggerProvider : ILoggerProvider
{

    public void Dispose()
    {
        //Do nothing
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new DebugbarLogger();
    }
}