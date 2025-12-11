using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RailwayMonitorClient.Interfaces;
using RailwayMonitorClient.Models.Entities;
using RailwayMonitorClient.Models.Enums;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace RailwayMonitorClient.Views;

/// <summary>
/// 导航栏
/// </summary>
public partial class AutoHideNavigationBar : UserControl
{
    public event Action<string> NavigationRequested;

    private DeviceManagement _deviceManagementWindow;
    private AlarmManagement _alarmManagementWindow;
    private PlayBack _playBackWindow;
    private IDeviceService? _deviceService;
    private IAlarmTraceService _alarmTraceService;

    public AutoHideNavigationBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 在Loaded事件中获取服务
        var app = Application.Current as App;
        if (app != null)
        {
            _deviceService = app.GetService<IDeviceService>();
            _alarmTraceService = app.GetRequiredService<IAlarmTraceService>();
        }
    }

    private void BtnRealTimeMonitor_Click(object sender, RoutedEventArgs e)
    {
        NavigationRequested?.Invoke("RealTimeMonitor");
    }

    private void BtnPlayback_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        if (mainWindow == null)
        {
            System.Windows.MessageBox.Show("无法获取主窗口引用", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_playBackWindow == null || !_playBackWindow.IsLoaded)
        {
            _playBackWindow = new PlayBack(mainWindow);
            _playBackWindow.Owner = mainWindow;
            _playBackWindow.Closed += (s, args) => _playBackWindow = null;
            _playBackWindow.Show();
        }
        else
        {
            _playBackWindow.Activate();
            if (_playBackWindow.WindowState == WindowState.Minimized)
            {
                _playBackWindow.WindowState = WindowState.Normal;
            }
        }

        NavigationRequested?.Invoke("Playback");
    }

    private void BtnDeviceManagement_Click(object sender, RoutedEventArgs e)
    {
        if (_deviceService == null)
        {
            MessageBox.Show("设备服务未初始化", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_deviceManagementWindow == null || !_deviceManagementWindow.IsLoaded)
        {
            _deviceManagementWindow = new DeviceManagement(_deviceService);
            _deviceManagementWindow.Owner = Window.GetWindow(this);
            _deviceManagementWindow.Closed += (s, args) => _deviceManagementWindow = null;
            _deviceManagementWindow.Show();
        }
        else
        {
            _deviceManagementWindow.Activate();
            if (_deviceManagementWindow.WindowState == WindowState.Minimized)
            {
                _deviceManagementWindow.WindowState = WindowState.Normal;
            }
        }

        NavigationRequested?.Invoke("DeviceManagement");
    }

    private void BtnAlarmManagement_Click(object sender, RoutedEventArgs e)
    {
        if (_alarmManagementWindow == null || !_alarmManagementWindow.IsLoaded)
        {
            _alarmManagementWindow = new AlarmManagement(_alarmTraceService);
            _alarmManagementWindow.Owner = Window.GetWindow(this);
            _alarmManagementWindow.Closed += (s, args) => _alarmManagementWindow = null;
            _alarmManagementWindow.Show();
        }
        else
        {
            _alarmManagementWindow.Activate();
            if (_alarmManagementWindow.WindowState == WindowState.Minimized)
            {
                _alarmManagementWindow.WindowState = WindowState.Normal;
            }
        }

        NavigationRequested?.Invoke("AlarmManagement");
    }

    private async void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        string deviceIp = "192.168.100.19";
        int channel = 34;
        int alarmType = 1;
       
        // 创建测试告警数据
        var testAlarm = new AlarmTrace
        {
            DeviceIp = deviceIp,
            SuperBrainChannel = channel,
            AlarmType = alarmType,
            AlarmDate = DateTime.Now,
            ImagePath = $@"alarmTraceImage\laoda.jpg",
            AlarmStatus = (int)EnumAlarmStatus.未处理,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now
        };

        // await alarmTraceService.AddAlarmTraceAsync(testAlarm);
        await _alarmTraceService.PushAlarmTraceAsync(testAlarm);
        // NavigationRequested?.Invoke("Settings");
    }

    private void BtnRefreshMonitor_Click(object sender, RoutedEventArgs e)
    {
        //TODO: Mainwindow监控页面刷新完毕才允许点击
        NavigationRequested?.Invoke("RefreshMonitor");
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        // 最小化主窗口
        var mainWindow = Window.GetWindow(this);
        if (mainWindow != null)
        {
            mainWindow.WindowState = WindowState.Minimized;
        }
    }

    /// <summary>
    /// 启用刷新监控按钮
    /// </summary>
    public void EnableRefreshMonitorButton()
    {
        BtnRefreshMonitor.IsEnabled = true;
    }
}