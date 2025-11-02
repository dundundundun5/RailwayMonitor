
using System.Windows;
using System.Windows.Threading;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Enums;
using RailwayAlarmBackend.Models.Utils;
using RailwayMonitorClient.Converters;
using RailwayMonitorClient.Services;

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
    /// 处理按钮点击事件
    /// </summary>
    private async void BtnHandle_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AlarmNotificationViewModel viewModel)
        {
            if (viewModel.SelectedAlarmStatus == 0)
            {
                System.Windows.MessageBox.Show("请先选择处理状态", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var alarmTraceHttpService = new AlarmTraceHttpService();
                var handleDto = new AlarmTraceHandleDto
                {
                    Id = _alarmTrace.Id,
                    AlarmHandleStatus = viewModel.SelectedAlarmStatus
                };

                var response = await alarmTraceHttpService.HandleAlarmTraceAsync(handleDto);

                if (response.Code == 200)
                {
                    // System.Windows.MessageBox.Show("处理成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 停止定时器
                    _timer.Stop();

                    // 关闭窗口
                    Close();
                }
                else
                {
                    System.Windows.MessageBox.Show(response.Message ?? "处理失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"处理预警时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
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

        // 预警状态
        AlarmStatus = alarmTrace.AlarmStatus;

        // 初始化状态下拉框列表
        AlarmStatusList = EnumResponseUtil.ToList<EnumAlarmStatus>().Where(response => response.Value > 0).ToList();
        SelectedAlarmStatus = 0; // 默认选择第一个选项
    }

    public int AlarmType { get; set; }
    public string DeviceInfo { get; set; }
    public string AlarmTime { get; set; }
    public string DisplayImagePath { get; set; }
    public int AlarmStatus { get; set; }

    /// <summary>
    /// 预警状态下拉框列表
    /// </summary>
    public List<EnumResponse> AlarmStatusList { get; set; }

    /// <summary>
    /// 选中的预警状态
    /// </summary>
    public int SelectedAlarmStatus { get; set; }

    /// <summary>
    /// 转换图片路径
    /// </summary>
    public static string BaseURL = "http://localhost:8081";
    /// <summary>
    /// 转换图片路径
    /// </summary>
    /// 
    private static string ConvertImagePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // 处理本地路径：./xxx/xxx.jpg -> xxx/xxx.jpg
        if (path.StartsWith("./"))
        {
            path = path.Substring(2);
        }

        // 处理超脑告警路径：xxx\xxx.jpg -> xxx/xxx.jpg
        path = path.Replace('\\', '/');

        // 移除开头的斜杠（如果有）
        if (path.StartsWith("/"))
        {
            path = path.Substring(1);
        }

        // 拼接BaseURL
        return $"{BaseURL}/{path}";
    }

}