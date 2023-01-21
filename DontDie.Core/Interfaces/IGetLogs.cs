using DontDie.Core.Events;

namespace DontDie.Core.Interfaces
{
    public interface IGetLogs
    {
        IEnumerable<string> Logs { get; }
        event EventHandler<LogAddedEventArgs> LogAdded;
    }
}
