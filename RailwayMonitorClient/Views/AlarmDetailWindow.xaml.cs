using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using RailwayAlarmBackend.Models.Enums;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Utils;
using RailwayMonitorClient.Services;

namespace RailwayMonitorClient.Views;

/// <summary>
/// AlarmDetailWindow.xaml 的交互逻辑
/// </summary>
public partial class AlarmDetailWindow : HandyControl.Controls.Window
{
    private readonly AlarmTraceHttpService _alarmTraceHttpService;
    private readonly DisplayAlarmTrace _alarmTrace;

    public AlarmDetailWindow(DisplayAlarmTrace alarmTrace)
    {
        InitializeComponent();
        _alarmTraceHttpService = new AlarmTraceHttpService();
        _alarmTrace = alarmTrace;
        DataContext = new AlarmDetailViewModel(alarmTrace);
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 处理按钮点击事件
    /// </summary>
    private async void BtnHandle_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AlarmDetailViewModel viewModel)
        {
            if (viewModel.SelectedAlarmStatus == 0)
            {
                // System.Windows.MessageBox.Show("请先选择处理状态", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var handleDto = new AlarmTraceHandleDto
                {
                    Id = _alarmTrace.Id,
                    AlarmHandleStatus = viewModel.SelectedAlarmStatus
                };

                var response = await _alarmTraceHttpService.HandleAlarmTraceAsync(handleDto);

                if (response.Code == 200)
                {
                    System.Windows.MessageBox.Show("处理成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 更新本地数据
                    _alarmTrace.AlarmStatus = viewModel.SelectedAlarmStatus;

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
}

/// <summary>
/// 预警详情视图模型
/// </summary>
public class AlarmDetailViewModel
{
    public AlarmDetailViewModel(DisplayAlarmTrace alarmTrace)
    {
        SuperBrainChannel = alarmTrace.SuperBrainChannel;
        AlarmType = alarmTrace.AlarmType;
        AlarmDate = alarmTrace.AlarmDate;
        AlarmStatus = alarmTrace.AlarmStatus;
        AlarmStatusText = GetAlarmStatusText(alarmTrace.AlarmStatus);
        DisplayImagePath = alarmTrace.DisplayImagePath;

        // 初始化状态下拉框列表
        AlarmStatusList = EnumResponseUtil.ToList<EnumAlarmStatus>().Where(response => response.Value > 0).ToList();
        SelectedAlarmStatus = 0; // 默认选择第一个选项
    }

    public int SuperBrainChannel { get; set; }
    public string AlarmType { get; set; }
    public DateTime AlarmDate { get; set; }
    public int AlarmStatus { get; set; }
    public string AlarmStatusText { get; set; }
    public string DisplayImagePath { get; set; }

    /// <summary>
    /// 预警状态下拉框列表
    /// </summary>
    public List<EnumResponse> AlarmStatusList { get; set; }

    /// <summary>
    /// 选中的预警状态
    /// </summary>
    public int SelectedAlarmStatus { get; set; }

    /// <summary>
    /// 获取预警状态文本
    /// </summary>
    private static string GetAlarmStatusText(int alarmStatus)
    {
        return alarmStatus switch
        {
            0 => "未处理",
            1 => "误报",
            2 => "容错",
            3 => "告警",
            _ => "未知"
        };
    }
}