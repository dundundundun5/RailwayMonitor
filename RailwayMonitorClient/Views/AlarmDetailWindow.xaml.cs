using System;
using System.Windows;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayMonitorClient.Views;

/// <summary>
/// AlarmDetailWindow.xaml 的交互逻辑
/// </summary>
public partial class AlarmDetailWindow : HandyControl.Controls.Window
{
    public AlarmDetailWindow(DisplayAlarmTrace alarmTrace)
    {
        InitializeComponent();
        DataContext = new AlarmDetailViewModel(alarmTrace);
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

/// <summary>
/// 预警详情视图模型
/// </summary>
public class AlarmDetailViewModel
{
    public static string BaseURL = "http://localhost:8081";
    public AlarmDetailViewModel(DisplayAlarmTrace alarmTrace)
    {
        SuperBrainChannel = alarmTrace.SuperBrainChannel;
        AlarmType = alarmTrace.AlarmType;
        AlarmDate = alarmTrace.AlarmDate;
        AlarmStatusText = GetAlarmStatusText(alarmTrace.AlarmStatus);
        DisplayImagePath = ImagePathTransfer(alarmTrace.DisplayImagePath);
    }
    // TODO: 弹窗直接处理告警
    public string ImagePathTransfer(string path)
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
    
    public int SuperBrainChannel { get; set; }
    public string AlarmType { get; set; }
    public DateTime AlarmDate { get; set; }
    public string AlarmStatusText { get; set; }
    public string DisplayImagePath { get; set; }

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