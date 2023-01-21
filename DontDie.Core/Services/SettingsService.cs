using DontDie.Core.Interfaces;

namespace DontDie.Core.Services;

public class SettingsService
{
    private readonly List<IObservableSettingsChanged> _subscribersToChanges;

    public Settings? Settings { get; protected set; }

    public SettingsService()
    {
        _subscribersToChanges = new();
    }

    public void AddSubscriberToChanges(IObservableSettingsChanged subscriber)
    {
        _subscribersToChanges.Add(subscriber);
    }

    public void RemoveSubscriberToChanges(IObservableSettingsChanged subscriber)
    {
        _subscribersToChanges.Remove(subscriber);
    }

    protected void NotifyChanges()
    {
        _subscribersToChanges.ForEach(sub => sub.SettingsChangedHandler(Settings!));
    }
}