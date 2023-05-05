using Hostbeat.Core.Enums;
using Hostbeat.Core.Events;
using Hostbeat.Core.Interfaces;
using System.Net;
using System.Timers;

namespace Hostbeat.Core.Services;

public class HeartbeatService : IObservableSettingsChanged, IHeartbeatCommands, IGetLogs, IDisposable
{
    private const int ONE_MINUTE_IN_MILLISECONDS = 60000;
    private readonly List<LogMessage> _logs;
    private readonly ILocale _locale;

    private Settings _settings = null!;
    private System.Timers.Timer _timer = null!;
    private bool disposedValue;

    public event EventHandler<LogAddedEventArgs> LogAdded = null!;

    public IEnumerable<LogMessage> Logs => _logs;

    public HeartbeatCommand? LastCommand { get; private set; }
    public bool StartedCommandFired { get; private set; }

    public HeartbeatService(ILocale locale)
	{
        _logs = new ();
        _locale = locale;
    }

    public void Configure(Settings settings)
    {
        _settings = settings;

        if (_timer != null)
        {
            return;
        }

        SetTimer(settings.Interval * ONE_MINUTE_IN_MILLISECONDS);
    }

    private void SetTimer(double interval)
    {
        _timer = new System.Timers.Timer(interval);
        _timer.Elapsed += OnTimedEvent;
        _timer.AutoReset = true;
    }

    void IObservableSettingsChanged.SettingsChangedHandler(Settings settings)
    {
        _settings = settings;

        if (_timer.Interval == settings.Interval * ONE_MINUTE_IN_MILLISECONDS)
        {
            return;
        }

        _timer.Interval = settings.Interval * ONE_MINUTE_IN_MILLISECONDS;
    }

    public async Task Start()
    {
        await RunWork();

        _timer.Start();

        LastCommand = HeartbeatCommand.Start;
        StartedCommandFired = true;
    }

    public void Stop()
    {
        _timer.Stop();

        LastCommand = HeartbeatCommand.Stop;
    }

    private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        string message = string.Format("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
        Console.WriteLine(message);

        await RunWork();
    }

    private async Task RunWork()
    {
        var result = await SendHeartbeat();

        _logs.Add(result);

        LogAdded?.Invoke(this, new LogAddedEventArgs(_logs.Count, result));

        if (_logs.Count > 3)
        {
            _logs.RemoveRange(0, 3);
        }
    }

    private async Task<LogMessage> SendHeartbeat()
    {
        using HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(Path.Combine(_settings.Url)),
            Timeout = TimeSpan.FromSeconds(15)
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/hosts/heartbeat");

        request.Headers.Add("Authorization", string.Format("Bearer {0}", _settings.Token));

        try
        {
            var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return LogMessage.CreateError(_locale.GetString("ErrorHostNotFound"));
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return LogMessage.CreateError(_locale.GetString("ErrorTokenNotValid"));
            }

            if (!response.IsSuccessStatusCode)
            {
                return LogMessage.CreateError(string.Format(_locale.GetString("ErrorUnknown"), response.StatusCode.ToString()));
            }
        }
        catch (Exception e)
        {
            return LogMessage.CreateError(e.Message);
        }

        return LogMessage.CreateSuccess(_locale.GetString("HeartbeatSuccess"));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _logs.Clear();
            }

            _timer?.Dispose();
            disposedValue = true;
        }
    }

    ~HeartbeatService()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
