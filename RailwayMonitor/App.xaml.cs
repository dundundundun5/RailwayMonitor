using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RailwayMonitor.Contexts;
using RailwayMonitor.Interfaces;
using RailwayMonitor.Services;
using Serilog;
using Application = System.Windows.Application;

namespace RailwayMonitor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        var builder = Host.CreateDefaultBuilder([]);
        builder.ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            });
        builder.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext());
        builder.ConfigureServices((context, services) =>
        {
            var version = context.Configuration.GetSection("SqlVersion").Value;
            if (version == null)
                version = "5.7.29";
                // 注册IConfiguration
            services.AddSingleton(context.Configuration);
            // 注册DbContext
            services.AddDbContext<DataContext>(options =>
                options.UseMySql(
                    context.Configuration.GetConnectionString("DefaultConnection")!,
                    new MySqlServerVersion(new Version(version)),
                    mysqlOptions => mysqlOptions
                        .EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(3),
                            errorNumbersToAdd: null
                        )
                ));

            // 注册配置
            services.Configure<Models.Configs.SuperBrainConfig>(context.Configuration.GetSection("SuperBrain"));

            // 注册服务
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IAlarmTraceService, AlarmTraceService>();

            // 注册后台托管服务
            services.AddHostedService<SuperBrainHostService>();

            // 注册主窗口 - 使用工厂方法创建
            services.AddSingleton(provider =>
            {
                var deviceService = provider.GetRequiredService<IDeviceService>();
                var alarmTraceService = provider.GetRequiredService<IAlarmTraceService>();
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new MainWindow(deviceService, alarmTraceService, configuration);
            });

            // 注册其他窗口 - 使用工厂方法创建
            services.AddTransient<Views.AlarmDetailWindow>(provider =>
            {
                // 这个窗口需要在创建时传递参数，所以这里只注册工厂
                // 实际创建时需要在代码中传递参数
                throw new InvalidOperationException(
                    "AlarmDetailWindow requires parameters. Use the factory method in code.");
            });

            services.AddTransient<Views.AlarmNotificationWindow>(provider =>
            {
                // 这个窗口需要在创建时传递参数
                throw new InvalidOperationException(
                    "AlarmNotificationWindow requires parameters. Use the factory method in code.");
            });

            services.AddTransient<Views.DeviceEditWindow>(provider =>
            {
                var deviceService = provider.GetRequiredService<IDeviceService>();
                return new Views.DeviceEditWindow(deviceService);
            });

            services.AddTransient<Views.AlarmManagement>(provider =>
            {
                var alarmTraceService = provider.GetRequiredService<IAlarmTraceService>();
                return new Views.AlarmManagement(alarmTraceService);
            });

            services.AddTransient<Views.DeviceManagement>(provider =>
            {
                var deviceService = provider.GetRequiredService<IDeviceService>();
                return new Views.DeviceManagement(deviceService);
            });

            services.AddTransient<Views.LiveView>();
            services.AddTransient<Views.PlayBack>(provider =>
            {
                var mainWindow = provider.GetRequiredService<MainWindow>();
                return new Views.PlayBack(mainWindow);
            });
        });
        _host = builder.Build();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // UI thread exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            Log.Error(args.Exception, "UI报错: {ErrorMessage}", args.Exception.Message);
            args.Handled = true;
        };
        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            Log.Error(args.Exception, "UI报错: {ErrorMessage}", args.Exception.Message);
            args.SetObserved();
        };

        // All unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            
            var exception = args.ExceptionObject as Exception;
            Log.Error(exception, "未处理异常: {ErrorMessage}", exception?.Message);
        };

        // 启动Host以运行后台托管服务
        _host.StartAsync().GetAwaiter().GetResult();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 确保日志被正确关闭
        Log.CloseAndFlush();

        // 释放Host资源
        _host.Dispose();

        base.OnExit(e);
    }

    public T? GetService<T>() where T : class
    {
        return _host.Services.GetService<T>();
    }

    public T GetRequiredService<T>() where T : class
    {
        return _host.Services.GetRequiredService<T>();
    }
}