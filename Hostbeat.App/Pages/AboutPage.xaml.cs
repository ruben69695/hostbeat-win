using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Windows.ApplicationModel;

namespace Hostbeat.Pages;

public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        this.InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var currentApp = (App)Application.Current;

        var aboutParagraph = new Paragraph();

        aboutParagraph.Inlines.Add(new Run() { Text = currentApp.Locale.GetString("AboutApp") });
        
        aboutText.Blocks.Add(aboutParagraph);

        
        string appName = currentApp.Locale.GetString("ms-resource:AppDisplayName");
        string version = GetStringVersion();
        versionLabel.Text = $"{appName} {version}";
    }

    private string GetStringVersion()
    {
        PackageVersion version = Package.Current.Id.Version;

        return $"{version.Major}.{version.Minor}.{version.Build}+{version.Revision}";
    }
}