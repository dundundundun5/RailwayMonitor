using System.Configuration;
using System.Data;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace RailwayMonitorClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // UI thread exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"图形界面报错: {args.Exception.Message}");
            args.Handled = true;
        };

        // All unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            MessageBox.Show($"报错: {(args.ExceptionObject as Exception)?.Message}");
        };

        // Task exceptions
        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            MessageBox.Show($"线程报错: {args.Exception.Message}");
            args.SetObserved();
        };
    }
}