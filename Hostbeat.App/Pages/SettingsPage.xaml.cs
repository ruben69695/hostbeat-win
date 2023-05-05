using Hostbeat.Core;
using Hostbeat.Core.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Text.RegularExpressions;
using Windows.System;

namespace Hostbeat.Pages;

public sealed partial class SettingsPage : Page
{
    private ISetSettings setSettings;
    private IGetSettings getSettings;
    private bool intervalIsValid;
    private bool tokenIsValid;
    private readonly Regex regexUrl;
    public SettingsPage()
    {
        regexUrl = new Regex(
            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
            RegexOptions.Compiled
        );
        this.InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var currentApp = (App)Application.Current;

        setSettings = currentApp.setSettings;
        getSettings = currentApp.getSettings;

        var settings = await getSettings.GetAsync();

        txtBoxToken.Text = settings.Token;
        txtBoxInterval.Text = settings.Interval.ToString();
        checkBoxAutoStart.IsChecked = settings.AutoStart;
    }

    private async void OnSaveButtonClicked(object sender, RoutedEventArgs e)
    {
        if (!intervalIsValid || !tokenIsValid)
        {
            return;
        }

        var token = txtBoxToken.Text;
        var interval = Convert.ToDouble(txtBoxInterval.Text);
        bool autoStart = checkBoxAutoStart.IsChecked ?? false;

        await setSettings.SetAsync(new Settings(token, interval, autoStart));
    }

    private void txtBoxInterval_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        var character = (char)e.OriginalKey;

        if (char.IsDigit(character) || character == 188 || e.OriginalKey == VirtualKey.Back)
        {
            e.Handled = false;
            return;
        }

        e.Handled = true;
    }

    private void OnIntervalChanges(object sender, TextChangedEventArgs e)
    {
        var source = sender as TextBox;

        double value = string.IsNullOrWhiteSpace(source.Text) 
            ? 0d 
            : Convert.ToDouble(source.Text);

        double min = 0.5d;
        double max = 3.0d;

        if (value < min || value > max)
        {
            infoInterval.Visibility = Visibility.Visible;
            infoInterval.Message = string.Format("Interval should be between {0} - {1}", min, max);
            infoInterval.IsOpen = true;

            intervalIsValid = false;
        }
        else
        {
            infoInterval.Visibility = Visibility.Collapsed;
            infoInterval.IsOpen = false;
            infoInterval.Message = string.Empty;

            intervalIsValid = true;
        }

        CheckToActivateSaveButton();
    }

    private void OnTokenChanged(object sender, TextChangedEventArgs e)
    {
        tokenIsValid = true;
        CheckToActivateSaveButton();
    }

    private void CheckToActivateSaveButton()
    {
        saveBtn.IsEnabled = tokenIsValid && intervalIsValid;
    }
}
