using DontDie.Core.Enums;
using DontDie.Core.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace DontDie.Pages
{
    public sealed partial class HeartbeatPage : Page
    {
        private IHeartbeatCommands _commands;
        private bool _started = false;

        public HeartbeatPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var currentApp = (App)Application.Current;
            var currentLogs = currentApp.GetLogs.Logs;

            _commands = currentApp.HeartbeatCommands;

            _started = _commands.LastCommand == HeartbeatCommand.Start;

            btnStartHeartbeat.Content = _started ? "Stop" : "Start";
            pBar.ShowPaused = _started ? false : true;

            foreach (var log in currentLogs)
            {
                txtLogger.Text += log.ToString() + Environment.NewLine;
            }

            currentApp.GetLogs.LogAdded += GetLogs_LogAdded;
        }

        private void GetLogs_LogAdded(object sender, Core.Events.LogAddedEventArgs e)
        {
            txtLogger.DispatcherQueue.TryEnqueue(() =>
            {
                if (e.Count + 1 == 21)
                {
                    txtLogger.Text = e.Log + Environment.NewLine;
                    return;
                }

                txtLogger.Text += e.Log + Environment.NewLine;
            });
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

            btnStartHeartbeat.Content = _started ? "Stop" : "Start";
            pBar.ShowPaused = _started ? false : true;
        }
    }
}
