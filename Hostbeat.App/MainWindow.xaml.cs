using Hostbeat.Core.Enums;
using Hostbeat.Core.Interfaces;
using Hostbeat.Pages;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.ApplicationModel.Activation;
using WinRT;
using WinRT.Interop;

namespace Hostbeat;

public sealed partial class MainWindow : Window
{
    private AppWindow m_AppWindow;
    private WindowsSystemDispatcherQueueHelper m_wsdqHelper;
    private MicaController m_micaController;
    private SystemBackdropConfiguration m_configurationSource;
    private IHeartbeatCommands commands;
    private App currentApp;
    private ILocale locale;
    private bool _forceClose;
    private bool _startMinimizedOnStartupTaskLaunch;

    public MainWindow()
    {
        this.InitializeComponent();

        currentApp = (App)Application.Current;

        m_AppWindow = GetAppWindowForCurrentWindow();
        m_AppWindow.Title = currentApp.Locale.GetString("AppDisplayName");
        m_AppWindow.Resize(new Windows.Graphics.SizeInt32(960, 960));

        CenterWindowInScreen(m_AppWindow);

        commands = currentApp.HeartbeatCommands;
        locale = currentApp.Locale;

        // Check to see if customization is supported.
        // Currently only supported on Windows 11.
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var titleBar = m_AppWindow.TitleBar;
            // Hide default title bar.
            titleBar.ExtendsContentIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        TrySetMicaBackdrop();

        _startMinimizedOnStartupTaskLaunch = WindowHasToStartMinimized();
    }

    private bool WindowHasToStartMinimized() 
        => Windows.ApplicationModel.AppInstance.GetActivatedEventArgs().Kind == ActivationKind.StartupTask && currentApp.getSettingsObject.AppStartupOnLogin == AppStartupValue.Minimized;

    private AppWindow GetAppWindowForCurrentWindow()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }

    private void MinimizeWindow(AppWindow window)
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        PInvoke.User32.ShowWindow(hWnd, PInvoke.User32.WindowShowStyle.SW_MINIMIZE);
    }

    private void CenterWindowInScreen(AppWindow window)
    {
        DisplayArea displayArea = DisplayArea.GetFromWindowId(window.Id, DisplayAreaFallback.Nearest);

        if (displayArea != null)
        {
            var centeredPosition = window.Position;

            centeredPosition.X = ((displayArea.WorkArea.Width - window.Size.Width) / 2);
            centeredPosition.Y = ((displayArea.WorkArea.Height - window.Size.Height) / 2);
            
            window.Move(centeredPosition);
        }
    }

    private bool TrySetMicaBackdrop()
    {
        if (MicaController.IsSupported())
        {
            m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

            // Hooking up the policy object
            m_configurationSource = new SystemBackdropConfiguration();
            this.Activated += Window_Activated;
            this.Closed += Window_Closed;
            ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            m_micaController = new MicaController
            {
                Kind = MicaKind.BaseAlt,

            };

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            m_micaController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
            m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
            return true; // succeeded
        }

        return false; // Mica is not supported on this system
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;

        if (_startMinimizedOnStartupTaskLaunch)
        {
            MinimizeWindow(m_AppWindow);
            _startMinimizedOnStartupTaskLaunch = false;
        }
    }

    private async void Window_Closed(object sender, WindowEventArgs args)
    {
        if (!_forceClose && commands.LastCommand == Core.Enums.HeartbeatCommand.Start)
        {
            var contentDialog = new ContentDialog();
            contentDialog.XamlRoot = this.Content.XamlRoot;
            contentDialog.Content = locale.GetString("ExitDialogContent");
            contentDialog.Title = locale.GetString("ExitDialogContentTitle");
            contentDialog.PrimaryButtonText = locale.GetString("ExitDialogPrimaryButtonText");
            contentDialog.CloseButtonClick += ContentDialog_CloseButtonClick;
            contentDialog.CloseButtonText = locale.GetString("ExitDialogCloseButtonText");
            args.Handled = true;
            await contentDialog.ShowAsync();
            return;
        }

        // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
        // use this closed window.
        if (m_micaController != null)
        {
            m_micaController.Dispose();
            m_micaController = null;
        }
        this.Activated -= Window_Activated;
        m_configurationSource = null;
    }

    private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _forceClose = true;
        Environment.Exit(0);
    }

    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        if (m_configurationSource != null)
        {
            SetConfigurationSourceTheme();
        }
    }

    private void SetConfigurationSourceTheme()
    {
        switch (((FrameworkElement)this.Content).ActualTheme)
        {
            case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
            case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
            case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
        }
    }

    private void OnNavigationChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var selectedItem = args.SelectedItem as NavigationViewItem;

        navigationView.Header = selectedItem.Content;

        Type page = null;

        page = (selectedItem.Tag as string) switch
        {
            "SettingsPage" => typeof(SettingsPage),
            "AboutPage" => typeof(AboutPage),
            _ => typeof(HeartbeatPage),
        };
        navigationContent.Navigate(page, null, new DrillInNavigationTransitionInfo());
    }
}
