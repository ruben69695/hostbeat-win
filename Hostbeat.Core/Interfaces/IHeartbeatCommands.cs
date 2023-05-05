using Hostbeat.Core.Enums;

namespace Hostbeat.Core.Interfaces;

public interface IHeartbeatCommands
{
    public HeartbeatCommand? LastCommand { get; }
    public bool StartedCommandFired { get; }
    Task Start();
    void Stop();
}
