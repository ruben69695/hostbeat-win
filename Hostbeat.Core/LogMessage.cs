using Hostbeat.Core.Enums;

namespace Hostbeat.Core;

public class LogMessage
{
    public DateTime Date { get; set; }
    public string Message { get; set; }
    public LogType Type { get; set; }

    public LogMessage(string message, LogType type, params string[] extras)
    {
        Date = DateTime.Now;
        Message = message;
        Type = type;
    }

    public static LogMessage CreateInfo(string message)
    {
        return new LogMessage(message, LogType.Information);
    }

    public static LogMessage CreateSuccess(string message)
    {
        return new LogMessage(message, LogType.Success);
    }

    public static LogMessage CreateWarning(string message)
    {
        return new LogMessage(message, LogType.Warning);
    }

    public static LogMessage CreateError(string message, params string[] extras)
    {
        return new LogMessage(message, LogType.Error, extras);
    }
}