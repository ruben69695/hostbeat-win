using DontDie.Core.Enums;

namespace DontDie.Core.Interfaces;

public interface IHeartbeatCommands
{
    public HeartbeatCommand? LastCommand { get; }
    Task Start();
    void Stop();
}
