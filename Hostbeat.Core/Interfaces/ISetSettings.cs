namespace Hostbeat.Core.Interfaces;

public interface ISetSettings
{
    Task SetAsync(Settings settings);
}
