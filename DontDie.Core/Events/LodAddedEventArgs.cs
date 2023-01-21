namespace DontDie.Core.Events;

public class LogAddedEventArgs
{
    public int Count { get; set; }
    public string Log { get; set; }
    public DateTime LogTime { get; set; }

    public LogAddedEventArgs(int count, string log, DateTime logTime)
    {
        Count = count;
        Log = log ?? throw new ArgumentNullException(nameof(log));
        LogTime = logTime;
    }
}