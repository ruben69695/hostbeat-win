using Hostbeat.Core.Enums;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Hostbeat.Services;

public interface IHostbeatAppStartupTaskService
{
    StartupTaskState? CurrentState { get; }
    Task<bool> ChangeStartupTaskStateAsync(AppStartupValue newValue);
    Task<StartupTaskState> GetCurrentState();
}

public class HostbeatAppStartupTaskService : IHostbeatAppStartupTaskService
{
    private const string TASK_ID = "HostbeatClientStartupTask";
    private StartupTaskState? _currentState;

    public StartupTaskState? CurrentState => _currentState;

    public async Task<StartupTaskState> GetCurrentState()
    {
        var task = await GetTaskAsync();

        _currentState = task.State;

        return task.State;
    }

    public async Task<bool> ChangeStartupTaskStateAsync(AppStartupValue newValue)
    {
        if (newValue == AppStartupValue.No)
        {
            return await DisableStartupTaskAsync();
        }

        return await EnableStartupTaskAsync();
    }

    private async Task<bool> EnableStartupTaskAsync()
    {
        var task = await GetTaskAsync();

        // Guard Clause
        if (IsTaskEnabled(task))
        {
            return true;
        }

        StartupTaskState? futureState = null;

        switch (task.State)
        {
            case StartupTaskState.Disabled:
                futureState = await task.RequestEnableAsync();
                Debug.WriteLine("Request to enable startup, result = {0}", futureState);
                break;
            case StartupTaskState.DisabledByUser:
                Debug.WriteLine("You have disabled this app's ability to run as soon as you sign in, but if you change your mind, you can enable this in the Startup tab in Task Manager.", "App Startup");
                break;
            case StartupTaskState.DisabledByPolicy:
                Debug.WriteLine("Startup disabled by group policy, or not supported on this device");
                break;
        }

        _currentState = futureState ?? task.State;

        return IsEnabledState(futureState ?? task.State);
    }

    private async Task<bool> DisableStartupTaskAsync()
    {
        var task = await GetTaskAsync();

        // Guard Clause
        if (IsTaskDisabled(task))
        {
            return true;
        }

        switch (task.State)
        {
            case StartupTaskState.Enabled:
                task.Disable();
                break;
            case StartupTaskState.EnabledByPolicy:
                Debug.WriteLine("Startup enabled by group policy, or not supported on this device");
                break;
        }

        _currentState = task.State;

        return IsDisabledState(task.State);
    }

    private async Task<StartupTask> GetTaskAsync() => await StartupTask.GetAsync(TASK_ID);
    private bool IsTaskEnabled(StartupTask task) => IsEnabledState(task.State);
    private bool IsTaskDisabled(StartupTask task) => IsDisabledState(task.State);
    private bool IsEnabledState(StartupTaskState state)
        => state == StartupTaskState.Enabled || state == StartupTaskState.EnabledByPolicy;
    private bool IsDisabledState(StartupTaskState state)
        => state == StartupTaskState.Disabled || state == StartupTaskState.DisabledByPolicy || state == StartupTaskState.DisabledByUser;
}
