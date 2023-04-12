using Hostbeat.Core.Interfaces;
using System.Text;

namespace Hostbeat.Core.Services;

public class SettingsFileService : SettingsService, IGetSettings, ISetSettings
{
	private readonly string _basePath;
	private readonly string _fileName;
	private readonly string _fullPath;
	private readonly Encoding _defaultEncoding;

	public SettingsFileService()
	{
		_basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "Hostbeat");
		_fileName = "settings.json";
		_fullPath = Path.Combine(_basePath, _fileName);
		_defaultEncoding = Encoding.UTF8;
    }

    async Task<Settings> IGetSettings.GetAsync(bool reloadCache)
    {
		if (reloadCache)
		{
			await ReadSettingsAsync();
		}

		return Settings!;
    }

    async Task ISetSettings.SetAsync(Settings settings)
    {
        Settings = settings;
		await WriteSettingsAsync();
    }

    public async Task ReadSettingsAsync()
	{
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
			await File.WriteAllTextAsync(_fullPath, Settings.Default().ToJson(), _defaultEncoding);
        }

		string content = await File.ReadAllTextAsync(_fullPath);
		
		Settings = Settings.LoadFromJson(content);
    }

    public async Task WriteSettingsAsync()
	{
		if (Settings is null)
		{
			throw new InvalidOperationException("Configuration is null, read the settings first");
		}

		if (!Directory.Exists(_basePath))
		{
			Directory.CreateDirectory(_basePath);
		}

		await File.WriteAllTextAsync(_fullPath, Settings.ToJson(), _defaultEncoding);

		NotifyChanges();
	}
}
