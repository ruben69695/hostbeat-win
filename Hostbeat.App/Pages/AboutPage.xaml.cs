using Hostbeat.Core.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

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
        var madeByParagraph = new Paragraph();

        aboutParagraph.Inlines.Add(new Run() { Text = currentApp.Locale.GetString("AboutApp") });
        madeByParagraph.Inlines.Add(new Run() { Text = currentApp.Locale.GetString("MadeBy") });
        
        aboutText.Blocks.Add(aboutParagraph);
        madeByText.Blocks.Add(madeByParagraph);
    }
}