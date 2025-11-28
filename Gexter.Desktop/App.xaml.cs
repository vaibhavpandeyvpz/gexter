using System.Text;
using System.Windows;

namespace Gexter.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Register code pages encoding provider for Windows-1252 support
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}
