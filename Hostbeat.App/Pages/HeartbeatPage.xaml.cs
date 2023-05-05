using Hostbeat.Core;
using Hostbeat.Core.Enums;
using Hostbeat.Core.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Hostbeat.Pages
{
    public sealed partial class HeartbeatPage : Page
    {
        private IHeartbeatCommands _commands;
        private bool _started = false;
        private ILocale _locale;
        private Settings _settings;

        public HeartbeatPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var currentApp = (App)Application.Current;
            var currentLogs = currentApp.GetLogs.Logs;

            _commands = currentApp.HeartbeatCommands;
            _locale = currentApp.Locale;
            _settings = currentApp.getSettingsObject;

            _started = _commands.LastCommand == HeartbeatCommand.Start;

            btnStartHeartbeat.Content = _started ? _locale.GetString("Stop") : _locale.GetString("Start");
            pBar.ShowPaused = _started ? false : true;

            foreach (var log in currentLogs)
            {
                CreateLogTemplate(mainGrid, log);
            }

            currentApp.GetLogs.LogAdded += GetLogs_LogAdded;

            if (!_commands.StartedCommandFired && _settings.AutoStart && !string.IsNullOrWhiteSpace(_settings.Token))
            {
                BtnStartClicked(this, new RoutedEventArgs());
            }
        }

        private void GetLogs_LogAdded(object sender, Core.Events.LogAddedEventArgs e)
        {
            mainGrid.DispatcherQueue.TryEnqueue(() =>
            {
                if (e.Count > 3)
                {
                    for (int i = mainGrid.Children.Count - 1; i > 5; i--)
                    {
                        mainGrid.Children.RemoveAt(i);
                    }

                    for (int i = mainGrid.RowDefinitions.Count - 1; i > 0; i--)
                    {
                        mainGrid.RowDefinitions.RemoveAt(i);
                    }
                }

                CreateLogTemplate(mainGrid, e.Message);
            });
        }

        private void CreateLogTemplate(Grid grid, LogMessage logMessage)
        {
            var acrylicBrush = (AcrylicBrush)Application.Current.Resources["AcrylicBackgroundFillColorDefaultBrush"];

            var dateBackground = new Border
            {
                Background = acrylicBrush,
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(left: 5, top: 0, right: 0, bottom: 0),
                RequestedTheme = ElementTheme.Default,
            };

            var dateBlock = new TextBlock
            {
                Padding = new Thickness(15),
                Text = logMessage.Date.ToString(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 15,
                RequestedTheme = ElementTheme.Default,
            };

            var messageBackground = new Border
            {
                Background = acrylicBrush,
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(left: 0, top: 0, right: 5, bottom: 0),
                RequestedTheme = ElementTheme.Default,
            };

            var messageBlock = new TextBlock
            {
                Padding = new Thickness(15),
                Text = logMessage.Message,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 15,
                RequestedTheme = ElementTheme.Default,
            };

            var symbolBackground = new Border
            {
                Background = acrylicBrush,
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(left: 0, top: 0, right: 5, bottom: 0),
                RequestedTheme = ElementTheme.Default,
            };

            var symbolPanel = new StackPanel
            {
                Padding = new Thickness(15),
                HorizontalAlignment = HorizontalAlignment.Left,
                RequestedTheme = ElementTheme.Default,
            };

            symbolPanel.Children.Add(new SymbolIcon(Symbol.Accept) { RequestedTheme = ElementTheme.Default});


            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.Children.Add(dateBackground);
            grid.Children.Add(dateBlock);
            Grid.SetRow(dateBackground, grid.RowDefinitions.Count - 1);
            Grid.SetRow(dateBlock, grid.RowDefinitions.Count - 1);
            Grid.SetColumn(dateBackground, 0);
            Grid.SetColumn(dateBlock, 0);
            Grid.SetColumnSpan(dateBackground, 2);


            grid.Children.Add(messageBackground);
            grid.Children.Add(messageBlock);
            Grid.SetRow(messageBackground, grid.RowDefinitions.Count - 1);
            Grid.SetRow(messageBlock, grid.RowDefinitions.Count - 1);
            Grid.SetColumn(messageBackground, 1);
            Grid.SetColumn(messageBlock, 1);
            Grid.SetColumnSpan(messageBackground, 2);

            grid.Children.Add(symbolBackground);
            grid.Children.Add(symbolPanel);
            Grid.SetRow(symbolBackground, grid.RowDefinitions.Count - 1);
            Grid.SetRow(symbolPanel, grid.RowDefinitions.Count - 1);
            Grid.SetColumn(symbolBackground, 2);
            Grid.SetColumn(symbolPanel, 2);
            Grid.SetColumnSpan(symbolBackground, 2);
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            var currentApp = (App)Application.Current;

            currentApp.GetLogs.LogAdded -= GetLogs_LogAdded;
        }

        private async void BtnStartClicked(object sender, RoutedEventArgs e)
        {
            if (_started)
            {
                _commands.Stop();
            }
            else
            {
                await _commands.Start();
            }

            _started = _commands.LastCommand == HeartbeatCommand.Start;

            btnStartHeartbeat.Content = _started ? _locale.GetString("Stop") : _locale.GetString("Start");
            pBar.ShowPaused = _started ? false : true;
        }
    }
}
