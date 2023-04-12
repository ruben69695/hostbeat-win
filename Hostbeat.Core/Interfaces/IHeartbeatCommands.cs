using Hostbeat.Core.Enums;

namespace Hostbeat.Core.Interfaces;

public interface IHeartbeatCommands
{
    public HeartbeatCommand? LastCommand { get; }
    Task Start();
    void Stop();
}
