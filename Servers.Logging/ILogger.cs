namespace Servers.Logging;

public enum LogState
{
    Info,
    Warning,
    Error
}

public interface ILogger<T> where T : ILogger<T>
{
    public static abstract T Start(DateTime start);
    
    public void Log(string source, LogState state, string message);

    public void Log(string source, Exception ex);
}