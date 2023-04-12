using System.Text.Json;

namespace Hostbeat.Core;

public class Settings
{
    public string Url { get; init; }
    public string Token { get; init; }
    public double Interval { get; init; }

    public Settings(string url, string token, double interval)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Interval = interval;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public static Settings LoadFromJson(string json)
    {
        return JsonSerializer.Deserialize<Settings>(json)!;
    }

    public static Settings Default()
    {
        return new ("https://hostbeatapi.rubenarrebola.pro", string.Empty, 1.0d);
    }
}
