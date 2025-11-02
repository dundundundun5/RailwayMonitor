using System.Windows;
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

    public AutoHideNavigationBar()
    {
        InitializeComponent();
    }

    private void BtnRealTimeMonitor_Click(object sender, RoutedEventArgs e)
    {
        NavigationRequested?.Invoke("RealTimeMonitor");
    }

    private void BtnPlayback_Click(object sender, RoutedEventArgs e)
    {
        if (_playBackWindow == null || !_playBackWindow.IsLoaded)
        {
            _playBackWindow = new PlayBack();
            _playBackWindow.Owner = Window.GetWindow(this);
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
        if (_deviceManagementWindow == null || !_deviceManagementWindow.IsLoaded)
        {
            _deviceManagementWindow = new DeviceManagement();
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
            _alarmManagementWindow = new AlarmManagement();
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

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        NavigationRequested?.Invoke("Settings");
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