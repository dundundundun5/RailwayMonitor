using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using RailwayAlarmBackend.Sdks;
using Microsoft.Extensions.Configuration;

namespace RailwayMonitorClient.Views;

/// <summary>
/// PlayBack.xaml 的交互逻辑
/// </summary>
public partial class PlayBack : HandyControl.Controls.Window
{
    private Recorder? _recorder;
    private List<string> _deviceList;
    private List<string> _ips;
    public PlayBack()
    {
        InitializeComponent();
        InitializeRecorder();
        InitializeDateTimeControls();
        Loaded += PlayBack_Loaded;
    }

    /// <summary>
    /// 窗口加载完成事件
    /// </summary>
    private void PlayBack_Loaded(object sender, RoutedEventArgs e)
    {
        // 在后台线程中自动登录并加载设备列表
        Task.Run(async () => await AutoLoginAndLoadDevices());
        
    }

    /// <summary>
    /// 自动登录并加载设备列表
    /// </summary>
    private async Task AutoLoginAndLoadDevices()
    {
        try
        {
            // 从appsettings.json获取RecorderIpAddress
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var recorderIpAddress = configuration["RecorderIpAddress"];

            if (string.IsNullOrEmpty(recorderIpAddress))
            {
                Dispatcher.Invoke(() =>
                {
                    TbLoginStatus.Text = "配置文件中未找到RecorderIpAddress";
                    TbLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
                });
                return;
            }

            // 解析IP地址和端口信息，格式：192.168.18.37:8000
            string ipAddress = recorderIpAddress;
            ushort port = 8000; // 默认端口8000

            if (recorderIpAddress.Contains(":"))
            {
                var parts = recorderIpAddress.Split(':');
                if (parts.Length == 2 && ushort.TryParse(parts[1], out ushort parsedPort))
                {
                    ipAddress = parts[0]; // 提取IP地址部分
                    port = parsedPort; // 提取端口号
                }
            }

            // 创建录像机实例
            _recorder = new Recorder(ipAddress, port);

            // 登录设备
            var result = _recorder.Login();

            if (string.IsNullOrEmpty(result))
            {
                Dispatcher.Invoke(() =>
                {
                    TbLoginStatus.Text = $"自动登录成功: {ipAddress}:{port}";
                    TbLoginStatus.Foreground = System.Windows.Media.Brushes.Green;
                });

                // 获取关联设备列表
                LoadAssociatedDevices();
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    TbLoginStatus.Text = $"自动登录失败: {result}";
                    TbLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
                });
            }
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
            {
                TbLoginStatus.Text = $"自动登录异常: {ex.Message}";
                TbLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
            });
        }
        finally
        {
            Dispatcher.Invoke(() => { CmbDevices.IsEnabled = true; });
        }
    }

    /// <summary>
    /// 初始化录像机实例
    /// </summary>
    private void InitializeRecorder()
    {
        // 创建默认的录像机实例
        _recorder = new Recorder("", 0);
        _deviceList = new List<string>();
        _ips = new List<string>();
    }

    /// <summary>
    /// 初始化日期时间控件
    /// </summary>
    private void InitializeDateTimeControls()
    {
        // 设置默认时间为当天1分钟内的录像
        var now = DateTime.Now;
        var startTime = now.AddMinutes(-1);

        DpStartDate.SelectedDate = startTime.Date;
        DpEndDate.SelectedDate = now.Date;

        TpStartTime.SelectedTime = startTime;
        TpEndTime.SelectedTime = now;
    }


    /// <summary>
    /// 加载关联设备列表
    /// </summary>
    private void LoadAssociatedDevices()
    {
        try
        {
            if (_recorder == null)
            {
                Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show("录像机实例未初始化", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return;
            }


            _recorder.GetAssociatedIpList(ref _ips, ref _deviceList);

            // 调试信息：检查获取到的数据
            System.Diagnostics.Debug.WriteLine($"获取到 {_ips?.Count ?? 0} 个IP地址");
            System.Diagnostics.Debug.WriteLine($"获取到 {_deviceList?.Count ?? 0} 个设备名称");

            // 使用Dispatcher更新UI
            Dispatcher.Invoke(() =>
            {
                // 直接使用设备名称列表
                CmbDevices.ItemsSource = _deviceList;

                System.Diagnostics.Debug.WriteLine($"加载了 {_deviceList?.Count ?? 0} 个设备名称");

                if (_deviceList?.Count > 0)
                {
                    CmbDevices.SelectedIndex = 0;
                    System.Diagnostics.Debug.WriteLine("已设置下拉框选中第一项");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("设备列表为空，无法设置选中项");
                }
            });
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show($"获取设备列表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    /// <summary>
    /// 开始回放按钮点击事件
    /// </summary>
    private void BtnStartPlayback_Click(object sender, RoutedEventArgs e)
    {
        if (_ips == null || _ips.Count == 0)
        {
            System.Windows.MessageBox.Show("没有可用的设备列表", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 检查是否有选中的设备
        if (CmbDevices.SelectedIndex < 0)
        {
            System.Windows.MessageBox.Show("请先选择设备", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 使用选中的设备进行回放
        var selectedIndex = CmbDevices.SelectedIndex;
        var selectedDevice = _deviceList[selectedIndex];
        var channel = (uint)(selectedIndex + 1); // 通道号 = 索引 + 1

        // 获取时间范围
        var startTime = GetStartDateTime();
        var endTime = GetEndDateTime();

        if (startTime >= endTime)
        {
            System.Windows.MessageBox.Show("开始时间必须小于结束时间", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (_recorder == null)
            {
                System.Windows.MessageBox.Show("录像机实例未初始化", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 获取PictureBox的句柄
            var handle = PictureBoxPlayback.Handle;

            
            // 开始回放（使用选中的通道）
            _recorder.StartPlayback(handle, startTime, endTime, channel);

            System.Windows.MessageBox.Show($"回放已开始: {selectedDevice} (通道{channel})", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"开始回放失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 停止回放按钮点击事件
    /// </summary>
    private void BtnStopPlayback_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_recorder == null)
            {
                System.Windows.MessageBox.Show("录像机实例未初始化", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _recorder.StopPlayback();

            // 清空PictureBox
            PictureBoxPlayback.Image = null;

            System.Windows.MessageBox.Show("回放已停止", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"停止回放失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 获取开始时间
    /// </summary>
    private DateTime GetStartDateTime()
    {
        var date = DpStartDate.SelectedDate ?? DateTime.Today;
        var time = TpStartTime.SelectedTime ?? DateTime.Now.AddMinutes(-1);

        return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
    }

    /// <summary>
    /// 获取结束时间
    /// </summary>
    private DateTime GetEndDateTime()
    {
        var date = DpEndDate.SelectedDate ?? DateTime.Today;
        var time = TpEndTime.SelectedTime ?? DateTime.Now;

        return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
    }

    /// <summary>
    /// 窗口关闭事件
    /// </summary>
    private void Window_Closed(object sender, EventArgs e)
    {
        // 停止回放并清理资源
        try
        {
            _recorder?.StopPlayback();

            // 清空PictureBox
            PictureBoxPlayback.Image = null;

            _recorder?.Dispose();
        }
        catch
        {
            // 忽略清理时的异常
        }
    }
}