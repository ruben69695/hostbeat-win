using DontDie.Core.Enums;
using DontDie.Core.Events;
using DontDie.Core.Interfaces;
using System.Net;
using System.Timers;

namespace DontDie.Core.Services;

public class HeartbeatService : IObservableSettingsChanged, IHeartbeatCommands, IGetLogs, IDisposable
{
    private const int ONE_MINUTE_IN_MILLISECONDS = 60000;
    private readonly List<string> _logs;

    private Settings _settings = null!;
    private System.Timers.Timer _timer = null!;
    private bool disposedValue;

    public event EventHandler<LogAddedEventArgs> LogAdded = null!;

    public IEnumerable<string> Logs => _logs;

    public HeartbeatCommand? LastCommand { get; private set; }

    public HeartbeatService()
	{
        _logs = new ();
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
        if (_logs.Count + 1 == 21)
        {
            _logs.Clear();
        }

        string result = await SendHeartbeat();

        _logs.Add(result);

        LogAdded?.Invoke(this, new LogAddedEventArgs(_logs.Count, result, DateTime.Now));
    }

    private async Task<string> SendHeartbeat()
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
               return string.Format("Error sending heartbeat at ({0}), host not found for current token", DateTime.Now);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return string.Format("Error sending heartbeat at ({0}), token not valid", DateTime.Now);
            }

            if (!response.IsSuccessStatusCode)
            {
                return string.Format("Unknown error sending heartbeat at ({0}), status code received: {1}", DateTime.Now, response.StatusCode);
            }
        }
        catch (Exception e)
        {
            return e.Message;
        }

        return string.Format("Heartbeat sended at ({0})", DateTime.Now);
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
