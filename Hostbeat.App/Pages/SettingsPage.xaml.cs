using Hostbeat.Core;
using Hostbeat.Core.Enums;
using Hostbeat.Core.Interfaces;
using Hostbeat.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Windows.System;

namespace Hostbeat.Pages;

public sealed partial class SettingsPage : Page
{
    private ISetSettings _setSettings;
    private IGetSettings _getSettings;
    private IHostbeatAppStartupTaskService _startupTaskService;
    private bool _intervalIsValid;
    private bool _tokenIsValid;
    private readonly Regex _regexUrl;
    private IEnumerable<StartupComboOption> _startupOptions;

    public SettingsPage()
    {
        _regexUrl = new Regex(
            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
            RegexOptions.Compiled
        );
        this.InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var currentApp = (App)Application.Current;

        _setSettings = currentApp.setSettings;
        _getSettings = currentApp.getSettings;
        _startupOptions = StartupComboOption.CreateOptions(currentApp.Locale);
        _startupTaskService = currentApp.getStartupTaskService;

        var settings = await _getSettings.GetAsync();

        txtBoxToken.Text = settings.Token;
        txtBoxInterval.Text = settings.Interval.ToString();
        checkBoxAutoStart.IsChecked = settings.AutoStart;
        comboAppStartup.ItemsSource = _startupOptions;
        comboAppStartup.SelectedIndex = getSelectedStartupOptionIndex(settings.AppStartupOnLogin);

        if (_startupTaskService.CurrentState == StartupTaskState.DisabledByUser || _startupTaskService.CurrentState == StartupTaskState.DisabledByPolicy)
        {
            comboAppStartup.IsEnabled = false;
            warningStartup.Visibility = Visibility.Visible;
            warningStartup.IsOpen = true;
        }

    }

    private int getSelectedStartupOptionIndex(AppStartupValue searchingValue)
    {
        int index = 0;
        bool found = false;

        while(!found && index < _startupOptions.Count())
        {
            if (_startupOptions.ElementAt(index).GetValue() == searchingValue)
            {
                found = true;
                continue;
            }

            index++;
        }

        return index;
    }

    private async void OnSaveButtonClicked(object sender, RoutedEventArgs e)
    {
        if (!_intervalIsValid || !_tokenIsValid)
        {
            return;
        }

        var token = txtBoxToken.Text;
        var interval = Convert.ToDouble(txtBoxInterval.Text);
        bool autoStart = checkBoxAutoStart.IsChecked ?? false;
        var appStartupValue = ((StartupComboOption)comboAppStartup.SelectedItem).GetValue();

        var value = await new HostbeatAppStartupTaskService().ChangeStartupTaskStateAsync(appStartupValue);
        await _setSettings.SetAsync(new Settings(token, interval, autoStart, appStartupValue));
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

            _intervalIsValid = false;
        }
        else
        {
            infoInterval.Visibility = Visibility.Collapsed;
            infoInterval.IsOpen = false;
            infoInterval.Message = string.Empty;

            _intervalIsValid = true;
        }

        CheckToActivateSaveButton();
    }

    private void OnTokenChanged(object sender, TextChangedEventArgs e)
    {
        _tokenIsValid = true;
        CheckToActivateSaveButton();
    }

    private void CheckToActivateSaveButton()
    {
        saveBtn.IsEnabled = _tokenIsValid && _intervalIsValid;
    }
}

public class StartupComboOption
{
    private AppStartupValue _value;

    public string DisplayValue { get; set; }

    public AppStartupValue GetValue() => _value;

    public StartupComboOption(string displayValue, AppStartupValue value)
    {
        _value = value;

        DisplayValue = displayValue;
    }

    public static IEnumerable<StartupComboOption> CreateOptions(ILocale locale)
    {
        return new[]
        {
            CreateMinimizedOption(locale),
            CreateYesOption(locale),
            CreateNoOption(locale),
        };
    }

    public static StartupComboOption CreateMinimizedOption(ILocale locale)
    {
        return new StartupComboOption(locale.GetString("MinimizedOptionDisplayName"), AppStartupValue.Minimized);
    }

    public static StartupComboOption CreateYesOption(ILocale locale)
    {
        return new StartupComboOption(locale.GetString("YesOptionDisplayName"), AppStartupValue.Yes);
    }

    public static StartupComboOption CreateNoOption(ILocale locale)
    {
        return new StartupComboOption(locale.GetString("NoOptionDisplayName"), AppStartupValue.No);
    }
}
