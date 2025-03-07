namespace Servers.Logging;

public enum LogState
{
    Info,
    Warning,
    Error
}

public interface ILogger
{
    public static abstract ILogger Start(DateTime start);
    
    public void Log(string source, LogState state, string message);

    public void Log(string source, Exception ex);
}

public static class LoggerExtensions
{
    public static SpecificLogger For(this ILogger self, string source) => new(self, source);
}

public class SpecificLogger
{
    public string Source { get; }
    private readonly ILogger _logger;
    
    public SpecificLogger(ILogger logger, string source)
    {
        _logger = logger;
        Source = source;
    }
    
    public void Info(string message) => _logger.Log(Source, LogState.Info, message);
    
    public void Warn(string message) => _logger.Log(Source, LogState.Warning, message);
    
    public void Error(string message) => _logger.Log(Source, LogState.Error, message);
    
    public void Error(Exception ex) => _logger.Log(Source, ex);
}