namespace Hostbeat.Core.Events;

public class LogAddedEventArgs
{
    public int Count { get; set; }
    public LogMessage Message { get; set; }

    public LogAddedEventArgs(int count, LogMessage logMessage)
    {
        Count = count;
        Message = logMessage;
    }
}