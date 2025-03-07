namespace Servers.Logging;

public class ConsoleLogger : ILogger<ConsoleLogger>
{
    private readonly DateTime _start;
    
    public ConsoleLogger(DateTime start) => _start = start;

    public static ConsoleLogger Start(DateTime start) => new(start);

    public void Log(string source, LogState state, string message)
    { 
        DateTime current = DateTime.UtcNow;
        Console.Write($"{current} {source}");
        Console.ForegroundColor = state.ToConsoleColor();
        Console.Write($" {state} ");
        Console.ResetColor();
        Console.WriteLine($"{message}");
    }

    public void Log(string source, Exception ex) => 
        Log(source, LogState.Error, ex.ToString());
}

internal static class LoggerStateExtensions
{
    public static ConsoleColor ToConsoleColor(this LogState self) => self switch
    {
        LogState.Error => ConsoleColor.Red,
        LogState.Warning => ConsoleColor.Yellow,
        LogState.Info => ConsoleColor.Green,
        _ => throw new NotSupportedException()
    };
}