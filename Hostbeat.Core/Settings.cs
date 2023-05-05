using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hostbeat.Core;

public class Settings
{
    [JsonIgnore]
    public string Url { get; private set; } = "https://hostbeatapi.rubenarrebola.pro";

    public string Token { get; init; }
    public double Interval { get; init; }
    public bool AutoStart {  get; init; }

    public Settings(string token, double interval, bool autoStart)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Interval = interval;
        AutoStart = autoStart;
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
        return new (string.Empty, 1.0d, false);
    }
}
