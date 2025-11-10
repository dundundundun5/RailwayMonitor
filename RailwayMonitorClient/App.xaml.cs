using System.Configuration;
using System.Data;
using System.Windows;
using Application = System.Windows.Application;
using Serilog;

namespace RailwayMonitorClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 初始化Serilog日志
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("Logs/app-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // UI thread exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            Log.Error(args.Exception, "图形界面报错: {ErrorMessage}", args.Exception.Message);
            args.Handled = true;
        };

        // All unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            Log.Error(exception, "未处理异常: {ErrorMessage}", exception?.Message);
        };

        // Task exceptions
        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            Log.Error(args.Exception, "线程报错: {ErrorMessage}", args.Exception.Message);
            args.SetObserved();
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 确保日志被正确关闭
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}