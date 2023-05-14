using Hostbeat.Core;
using Hostbeat.Core.Interfaces;
using Hostbeat.Core.Services;
using Hostbeat.Locale;
using Hostbeat.Services;
using Microsoft.UI.Xaml;

namespace Hostbeat
{
    public partial class App : Application
    {
        private Window m_window;
        private readonly SettingsFileService settingsService;
        private readonly HeartbeatService heartbeatService;
        private readonly LocaleService localeService;
        private readonly HostbeatAppStartupTaskService startupTaskService;

        public ISetSettings setSettings => settingsService;
        public IGetSettings getSettings => settingsService;
        public IGetLogs GetLogs => heartbeatService;
        public IHeartbeatCommands HeartbeatCommands => heartbeatService;
        public ILocale Locale => localeService;
        public Settings getSettingsObject => settingsService.Settings;
        public IHostbeatAppStartupTaskService getStartupTaskService => startupTaskService;

        public App()
        {
            this.InitializeComponent();
            localeService = new LocaleService(Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse());
            settingsService = new SettingsFileService();
            heartbeatService = new HeartbeatService(localeService);
            startupTaskService = new HostbeatAppStartupTaskService();
        }

        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            await settingsService.ReadSettingsAsync();
            await startupTaskService.GetCurrentState();
            settingsService.AddSubscriberToChanges(heartbeatService);

            heartbeatService.Configure(settingsService.Settings);

            m_window = new MainWindow();
            m_window.Activate();

        }

    }
}
