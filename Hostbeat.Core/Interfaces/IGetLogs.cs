using Hostbeat.Core.Events;

namespace Hostbeat.Core.Interfaces
{
    public interface IGetLogs
    {
        IEnumerable<LogMessage> Logs { get; }
        event EventHandler<LogAddedEventArgs> LogAdded;
    }
}
