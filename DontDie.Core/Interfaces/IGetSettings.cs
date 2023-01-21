namespace DontDie.Core.Interfaces;

public interface IGetSettings
{
    Task<Settings> GetAsync(bool reloadCache = false);
}
