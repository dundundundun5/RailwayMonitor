
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using HK.Net.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;
using RailwayAlarmBackend.Sdks;
using RailwayMonitorClient.Controls;
using RailwayMonitorClient.Services;
using RailwayMonitorClient.Views;
using MessageBox = System.Windows.MessageBox;

namespace RailwayMonitorClient;

/// <summary>
/// 摄像头窗口数据模型
/// </summary>
public class CameraWindowData
{
    public LiveView CameraWindow { get; set; }
    public string IpAddress { get; set; }
    public int Channel { get; set; }
    public ushort Port { get; set; }
    public int Index { get; set; }
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public static IConfiguration Configuration { get; set; }
    private List<CameraWindowData> _cameraData;
    private AlarmHubService _alarmHubService;

    public MainWindow()
    {
        InitializeComponent();
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        CHCNetSDK.NET_DVR_Init();
        // 使用HTTP方式初始化摄像头窗口
        Task.Run(async () =>
        {
            await InitializeCameraWindowsWithHttp();
            // 摄像头窗口初始化完成后启动预览
            StartPreviewAll();
        });
        InitializeSignalR();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// 初始化SignalR连接
    /// </summary>
    private async void InitializeSignalR()
    {
        try
        {
            _alarmHubService = new AlarmHubService();

            // 注册告警接收事件
            _alarmHubService.OnAlarmReceived += OnAlarmReceived;

            // 启动SignalR连接
            await _alarmHubService.StartAsync();

            Console.WriteLine("SignalR连接已成功建立");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR连接失败: {ex.Message}");
            // 可以在这里添加重试逻辑
        }
    }

    /// <summary>
    /// 接收到告警推送的处理方法
    /// </summary>
    private void OnAlarmReceived(AlarmTrace alarmTrace)
    {
        
        // 在UI线程中显示通知
        Dispatcher.Invoke(() =>
        {
            Console.WriteLine($"收到告警{alarmTrace.AlarmDate}");
            ShowAlarmNotification(alarmTrace);
        });
    }

    /// <summary>
    /// 显示告警通知窗口
    /// </summary>
    private void ShowAlarmNotification(AlarmTrace alarmTrace)
    {
        try
        {
            var notificationWindow = new AlarmNotificationWindow(alarmTrace);
            notificationWindow.Show();

            Console.WriteLine($"收到告警推送: {alarmTrace.AlarmType} - {alarmTrace.DeviceIp}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"显示告警通知失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 窗口关闭事件
    /// </summary>
    private async void MainWindow_Closed(object sender, EventArgs e)
    {
        if (_alarmHubService != null)
        {
            await _alarmHubService.StopAsync();
        }
    }
    //TODO: 后端没起来就点刷新监控会报错
    
    private void InitializeCameraWindows()
    {
        _cameraData = new List<CameraWindowData>();

        int total = int.Parse(Configuration.GetSection("VideoNumber").Value);
        var ips = Configuration.GetSection("VideoIpAddressList").GetChildren().Select(x => x.Value).ToArray();

        // 创建一个LiveView控件来容纳所有摄像头
        var liveView = new LiveView();

        // 设置LiveView填充整个剩余空间
        Grid.SetRow(liveView, 1);
        Grid.SetColumn(liveView, 0);
        Grid.SetColumnSpan(liveView, 4);

        // 添加到主网格
        MainGrid.Children.Add(liveView);

        for (int i = 0; i < total; i++)
        {
            string ipAddress = i < ips.Length ? ips[i] : null;
            int channel = 1; // 默认通道1
            ushort port = 8000; // 默认端口8000

            // 解析IP地址中的端口和通道信息，格式：192.168.18.37:8000:1
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(":"))
            {
                var parts = ipAddress.Split(':');

                if (parts.Length == 3) // IP:PORT:CHANNEL 格式
                {
                    ipAddress = parts[0]; // 提取IP地址部分
                    if (ushort.TryParse(parts[1], out ushort parsedPort))
                    {
                        port = parsedPort; // 提取端口号
                    }
                    if (int.TryParse(parts[2], out int parsedChannel))
                    {
                        channel = parsedChannel; // 提取通道号
                    }
                }
                else if (parts.Length == 2) // IP:CHANNEL 格式（向后兼容）
                {
                    ipAddress = parts[0]; // 提取IP地址部分
                    if (int.TryParse(parts[1], out int parsedChannel))
                    {
                        channel = parsedChannel; // 提取通道号
                    }
                }

                // 配置文件方式无法获取设备类型，但根据通道号判断
                // 如果通道号>=33，说明已经是多通道设备的通道号，不需要再加32
                // 如果通道号<33，可能是普通摄像头的通道号，保持不变
                // 注意：配置文件方式无法区分设备类型，所以这里保持原样
            }

            _cameraData.Add(new CameraWindowData
            {
                CameraWindow = liveView,
                IpAddress = ipAddress,
                Channel = channel,
                Port = port,
                Index = i
            });
        }
    }

    /// <summary>
    /// 通过HTTP动态查询后端设备列表初始化摄像头窗口
    /// </summary>
    private async Task InitializeCameraWindowsWithHttp()
    {
        _cameraData = new List<CameraWindowData>();

        try
        {
            var deviceHttpService = new DeviceHttpService();
            var queryDto = new DeviceQueryDto { HasChannel = true };

            var response = await deviceHttpService.QueryDevicesAsync(queryDto);

            if (response.Code == 200 && response.Data != null)
            {
                // 按设备名称从小到大排序
                var devices = response.Data
                    .Where(d => d.Enabled == 1 && !string.IsNullOrEmpty(d.Ip))
                    .OrderBy(d => d.Name)
                    .ToList();

                // 在UI线程中创建和添加控件
                await Dispatcher.InvokeAsync(() =>
                {
                    // 创建一个LiveView控件来容纳所有摄像头
                    var liveView = new LiveView();

                    // 设置LiveView填充整个剩余空间
                    Grid.SetRow(liveView, 1);
                    Grid.SetColumn(liveView, 0);
                    Grid.SetColumnSpan(liveView, 4);

                    // 添加到主网格
                    MainGrid.Children.Add(liveView);

                    for (int i = 0; i < devices.Count && i < 12; i++) // 最多显示12个摄像头
                    {
                        var device = devices[i];

                        // 计算实际通道号
                        int actualChannel = device.Channel;
                        if (device.Type != (int)EnumDeviceType.摄像机)
                        {
                            // 录像机和超脑的通道号需要+32，第一个数字通道是33
                            actualChannel = device.Channel + 32;
                        }

                        _cameraData.Add(new CameraWindowData
                        {
                            CameraWindow = liveView,
                            IpAddress = device.Ip,
                            Channel = actualChannel,
                            Index = i,
                            Port = (ushort)device.Port
                        });
                    }

                    Console.WriteLine($"成功加载 {_cameraData.Count} 个摄像头");
                });
            }
            else
            {
                Console.WriteLine($"获取设备列表失败: {response.Message}");
                // 如果HTTP请求失败，回退到配置文件方式
                await Dispatcher.InvokeAsync(() => InitializeCameraWindows());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HTTP获取设备列表异常: {ex.Message}");
            // 如果发生异常，回退到配置文件方式
            await Dispatcher.InvokeAsync(() => InitializeCameraWindows());
        }
    }


    

    /// <summary>
    /// 异步启动摄像头预览（线程安全版本）
    /// </summary>
    private async Task StartCameraPreviewAsync(LiveView control, string ip, int channel, int cameraIndex, ushort port = 8000)
    {
        try
        {
            // 在UI线程中获取PictureBox句柄
            IntPtr handle = await Dispatcher.InvokeAsync(() => control.GetPictureBoxHandle(cameraIndex));
            if (handle == IntPtr.Zero)
                return;

            Camera camera = new Camera(cameraIpAddress: ip, port: port, realPlayHandle: handle);

            // 在UI线程中设置相机服务
            await Dispatcher.InvokeAsync(() =>
            {
                control.Camera = camera;
            });

            // 登录和启动预览可以在后台线程执行
            camera.Login();
            camera.StartPreview(channel: channel, streamType:EnumStreamType.子码流, linkMode:EnumLinkMode.TCP);

            // 在UI线程中设置别名
            await Dispatcher.InvokeAsync(() =>
            {
                Console.WriteLine($"摄像头 {cameraIndex + 1} - {ip}:{port} (通道{channel})");
                control.SetAlias(cameraIndex, $"摄像头 {cameraIndex + 1} - {ip}:{port} (通道{channel})");
            });
        }
        catch (Exception ex)
        {
            // 在UI线程中显示错误信息
            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show($"摄像头 {cameraIndex + 1} 初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }
    

    private async void StartPreviewAll()
    {
        if (_cameraData == null || _cameraData.Count == 0)
        {
            Console.WriteLine("没有可用的摄像头数据，跳过预览启动");
            return;
        }

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 3 // 限制同时加载的摄像头数量，避免资源耗尽
        };

        // 使用 Parallel.ForEachAsync 并行加载所有摄像头
        await Parallel.ForEachAsync(_cameraData, parallelOptions, async (cameraData, cancellationToken) =>
        {
            if (cameraData.IpAddress != null)
            {
                // 在后台线程中执行摄像头初始化
                await StartCameraPreviewAsync(cameraData.CameraWindow, cameraData.IpAddress, cameraData.Channel, cameraData.Index, cameraData.Port);
            }
        });
    }

    /// <summary>
    /// 停止所有摄像头预览
    /// </summary>
    private void StopAllPreviews()
    {
        if (_cameraData == null || _cameraData.Count == 0)
        {
            Console.WriteLine("没有正在预览的摄像头");
            return;
        }

        // 清理所有摄像头资源
        foreach (var cameraData in _cameraData)
        {
            if (cameraData.CameraWindow?.Camera != null)
            {
                try
                {
                    cameraData.CameraWindow.Camera.Dispose();
                    cameraData.CameraWindow.Camera = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"停止摄像头预览异常: {ex.Message}");
                }
            }
        }

        Console.WriteLine("已停止所有摄像头预览");
    }

    private void NavigationBar_NavigationRequested(string page)
    {
        switch (page)
        {
            case "RealTimeMonitor":
                // 实时监控 - 默认就是主窗口
                // 可以在这里添加实时监控的特定逻辑
                break;
            case "Playback":
                // 录像回放
                // var playbackWindow = new RecorderWindow();
                // playbackWindow.Owner = this;
                // playbackWindow.Show();
                break;
            case "DeviceManagement":
                // 设备管理 - 已经在导航栏中处理
                break;
            case "AlarmManagement":
                // 预警管理
                break;
            case "Settings":
                // 设置
                MessageBox.Show("系统设置功能开发中...", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                break;
            case "RefreshMonitor":
                // 刷新监控
                RefreshMonitor();
                break;
        }
    }

    /// <summary>
    /// 刷新监控 - 停止预览并重新初始化摄像头窗口
    /// </summary>
    private void RefreshMonitor()
    {
        try
        {
            Console.WriteLine("开始刷新监控...");

            // 停止所有预览
            StopAllPreviews();

            // 清理现有摄像头数据
            _cameraData?.Clear();

            // 清理LiveView控件
            Dispatcher.Invoke(() =>
            {
                // 移除现有的LiveView控件
                var existingLiveView = MainGrid.Children.OfType<LiveView>().FirstOrDefault();
                if (existingLiveView != null)
                {
                    MainGrid.Children.Remove(existingLiveView);
                    existingLiveView.Cleanup();
                }
            });

            // 重新初始化摄像头窗口
            Task.Run(async () =>
            {
                await InitializeCameraWindowsWithHttp();
                // 摄像头窗口初始化完成后启动预览
                StartPreviewAll();
            });

            Console.WriteLine("监控刷新完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"刷新监控失败: {ex.Message}");
            MessageBox.Show($"刷新监控失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }




    

    
}