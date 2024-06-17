namespace MCBA.Debugbar;

public class Debugbar
{
    public static void Message(string type, string message)
    {
        Kernel.Instance().AddMessage(type,message);
    }
    
    public static void Message(string message)
    {
        Kernel.Instance().AddMessage("Debug",message);
    }

    public static List<string[]> GetMessages()
    {
        return Kernel.Instance().GetMessages();
    }

    public static string GetVersion()
    {
        return Kernel.Instance().GetCurrentVersion();
    }

    public static long GetMemoryUsage()
    {
        return Kernel.Instance().CalculateTotalMemory();
    }

    public static List<Exception> GetExceptions()
    {
        return Kernel.Instance().GetExceptions();
    }

    public static List<string[]> GetQueries()
    {
        return Kernel.Instance().GetQueries();
    }

    public static void Boot()
    {
        Kernel.Boot();
    }

    public static HttpRequest GetRequest()
    {
        return Kernel.Instance().GetRequest();
    }

    public static HttpResponse GetResponse()
    {
        return Kernel.Instance().GetResponse();
    }

    public static ISession GetSession()
    {
        return Kernel.Instance().GetSession();
    }

    public static void SetApp(WebApplication app)
    {
        Kernel.Instance().SetApp(app);
    }

    public static bool ShouldRender()
    {
        return Kernel.Instance().IsDevelopment();
    }

    public static long CalculateRequestDuration()
    {
        return Kernel.Instance().CalculateRequestDuration();
    }
}