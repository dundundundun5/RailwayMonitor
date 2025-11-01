using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using RailwayAlarmBackend.Models.Entities;
using RailwayMonitorClient.Converters;

namespace RailwayMonitorClient.Views;

/// <summary>
/// AlarmNotificationWindow.xaml 的交互逻辑
/// </summary>
public partial class AlarmNotificationWindow : HandyControl.Controls.Window
{
    private readonly DispatcherTimer _timer;
    private int _remainingSeconds = 60;
    private readonly AlarmTrace _alarmTrace;

    public AlarmNotificationWindow(AlarmTrace alarmTrace)
    {
        InitializeComponent();
        _alarmTrace = alarmTrace;

        // 设置窗口位置（右下角）
        SetWindowPosition();

        // 初始化数据
        InitializeData();

        // 启动定时器
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    /// <summary>
    /// 设置窗口位置到右下角
    /// </summary>
    private void SetWindowPosition()
    {
        // 获取主屏幕的工作区域
        var screenWidth = SystemParameters.WorkArea.Width;
        var screenHeight = SystemParameters.WorkArea.Height;

        // 设置窗口位置（右下角，留出一些边距）
        Left = screenWidth - Width - 20;
        Top = screenHeight - Height - 20;
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    private void InitializeData()
    {
        // 创建视图模型
        var viewModel = new AlarmNotificationViewModel(_alarmTrace);
        DataContext = viewModel;

        // 更新计时器显示
        UpdateTimerDisplay();
    }

    /// <summary>
    /// 定时器事件
    /// </summary>
    private void Timer_Tick(object sender, EventArgs e)
    {
        _remainingSeconds--;
        UpdateTimerDisplay();

        if (_remainingSeconds <= 0)
        {
            _timer.Stop();
            Close();
        }
    }

    /// <summary>
    /// 更新计时器显示
    /// </summary>
    private void UpdateTimerDisplay()
    {
        TbTimer.Text = $"{_remainingSeconds}s";
    }

    /// <summary>
    /// 查看详情按钮点击事件
    /// </summary>
    private void BtnViewDetail_Click(object sender, RoutedEventArgs e)
    {
        // 停止定时器
        _timer.Stop();

        // 打开预警管理页面
        var alarmManagement = new AlarmManagement();
        alarmManagement.Show();

        // 关闭通知窗口
        Close();
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        Close();
    }

    /// <summary>
    /// 窗口加载完成事件
    /// </summary>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // 窗口加载时自动激活
        Activate();
    }

    /// <summary>
    /// 窗口关闭事件
    /// </summary>
    private void Window_Closed(object sender, EventArgs e)
    {
        _timer?.Stop();
    }
}

/// <summary>
/// 告警通知视图模型
/// </summary>
public class AlarmNotificationViewModel
{
    public AlarmNotificationViewModel(AlarmTrace alarmTrace)
    {
        AlarmType = alarmTrace.AlarmType;

        // 使用通道转换器将通道号转换为点位名称
        int pointNumber = ChannelToPointConverter.GetPointNumber(alarmTrace.SuperBrainChannel);
        DeviceInfo = $"设备: {alarmTrace.DeviceIp} 点位: {pointNumber}号点位";

        AlarmTime = alarmTrace.AlarmDate.ToString("yyyy-MM-dd HH:mm:ss");
        DisplayImagePath = ConvertImagePath(alarmTrace.ImagePath);
    }

    public int AlarmType { get; set; }
    public string DeviceInfo { get; set; }
    public string AlarmTime { get; set; }
    public string DisplayImagePath { get; set; }

    /// <summary>
    /// 转换图片路径
    /// </summary>
    private static string ConvertImagePath(string originalPath)
    {
        if (string.IsNullOrEmpty(originalPath))
            return null;

        // 将 "alarmTraceImage\\xxx.jpg" 转换为 "http://localhost:8081/alarmTraceImage/xxx.jpg"
        var fileName = System.IO.Path.GetFileName(originalPath);
        return $"http://localhost:8081/alarmTraceImage/{fileName}";
    }

}