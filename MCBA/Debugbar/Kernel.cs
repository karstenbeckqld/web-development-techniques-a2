using MCBA.Debugbar.Logging;

namespace MCBA.Debugbar;

public class Kernel
{
    private string _version;
    private bool _isDevelopment;
    private List<string[]> _messages;
    private List<Exception> _exceptions;
    private List<string[]> _queries;
    private WebApplication _app;
    private long _requestStartTime;
    private HttpRequest _request;
    private HttpResponse _response;
    private ISession _session;
    
    private static Kernel _instance;
    
    public Kernel()
    {
        _version = Environment.Version.ToString();
        _messages = new List<string[]>();
        _exceptions = new List<Exception>();
        _queries = new List<string[]>();
    }

    public string GetCurrentVersion()
    {
        return _version;
    }

    public List<string[]> GetMessages()
    {
        _messages.Reverse();
        return _messages;
    }

    public void AddMessage(string type, string message)
    {
        string[] newMessage = {DateTime.Now.ToString(),type,message,DebugbarLogger.colors[type]};
        _messages.Add(newMessage);
    }

    public void AddQuery(string type, string message)
    {
        string[] newQuery = { DateTime.Now.ToString(), type, message };
        _queries.Add(newQuery);
    }
    

    public List<string[]> GetQueries()
    {
        _queries.Reverse();
        return _queries;
    }
    public long CalculateTotalMemory()
    {
        return  GC.GetTotalMemory(false) / (1024 * 1024);
    }

    public List<Exception> GetExceptions()
    {
        return _exceptions;
    }

    public bool IsDevelopment()
    {
        return _isDevelopment;
    }

    public long CalculateRequestDuration()
    {
        return (DateTime.Now.Millisecond - _requestStartTime);
    }

    public void Reset()
    {
        _requestStartTime = DateTime.Now.Millisecond;
    }

    public ISession GetSession()
    {
        return _session;
    }

    public void SetApp(WebApplication app)
    {
        _isDevelopment = app.Environment.IsDevelopment();
        _app = app;
        //Reset the debugbar every time a new request is processed
        app.Use((context, next) =>
        {
            _instance.Reset();
            _request = context.Request;
            _response = context.Response;

            return next.Invoke();
        });
    }

    public void AddException(Exception exception)
    {
        _exceptions.Add(exception);
    }

    public HttpRequest GetRequest()
    {
        return _request;
    }

    public HttpResponse GetResponse()
    {
        return _response;
    }
    

    public static void Boot()
    {
        _instance = new Kernel();
    }

    public static Kernel Instance()
    {
        return _instance;
    }
    
    


}