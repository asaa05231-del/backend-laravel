using System.IO;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SistemPengirimanApp.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        this.UnhandledException += (s, e) =>
        {
            e.Handled = true;
            LogCrash(e.Exception);
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                LogCrash(ex);
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            LogCrash(e.Exception);
            e.SetObserved();
        };
    }

    private void LogCrash(Exception ex)
    {
        var errorText = $"[{DateTime.Now}]\n{ex}\n\n";
        File.AppendAllText(Path.Combine(Path.GetTempPath(), "lemburapp_crash.log"), errorText);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

