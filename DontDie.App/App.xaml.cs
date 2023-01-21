using DontDie.Core.Interfaces;
using DontDie.Core.Services;
using Microsoft.UI.Xaml;

namespace DontDie
{
    public partial class App : Application
    {
        private Window m_window;
        private readonly SettingsFileService settingsService;
        private readonly HeartbeatService heartbeatService;

        public ISetSettings setSettings => settingsService;
        public IGetSettings getSettings => settingsService;
        public IGetLogs GetLogs => heartbeatService;
        public IHeartbeatCommands HeartbeatCommands => heartbeatService;

        public App()
        {
            this.InitializeComponent();
            settingsService = new SettingsFileService();
            heartbeatService = new HeartbeatService();
        }

        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            await settingsService.ReadSettingsAsync();
            settingsService.AddSubscriberToChanges(heartbeatService);

            heartbeatService.Configure(settingsService.Settings);

            m_window = new MainWindow();
            m_window.Activate();
        }

    }
}
