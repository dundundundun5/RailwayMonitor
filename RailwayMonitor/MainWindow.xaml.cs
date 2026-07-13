using System.Text;
using System.Windows;
using System.Windows.Controls;
using HK.Net.Core;
using Microsoft.Extensions.Configuration;
using RailwayMonitor.Interfaces;
using RailwayMonitor.Models.Dtos;
using RailwayMonitor.Models.Entities;
using RailwayMonitor.Models.Enums;
using RailwayMonitor.Sdks;
using RailwayMonitor.Views;
using Serilog;
using MessageBox = System.Windows.MessageBox;

namespace RailwayMonitor;

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
    private readonly IDeviceService _deviceService;

    private readonly IAlarmTraceService _alarmTraceService;
    // 录像机管理相关属性
    private Recorder? _recorder;
    private List<string> _deviceList;
    private List<string> _ips;
    private List<int> _channels;
    private bool _isRecorderInitialized = false;

    public MainWindow(IDeviceService deviceService, IAlarmTraceService alarmTraceService,IConfiguration configuration)
    {
        InitializeComponent();

        _deviceService = deviceService;
        _alarmTraceService = alarmTraceService;
        _alarmTraceService.AlarmReceived += OnAlarmReceived;
        Configuration = configuration;
        CHCNetSDK.NET_DVR_Init();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // 初始化录像机
        Thread.Sleep(5000);
        InitializeRecorderAsync();
        Thread.Sleep(5000);
        // 使用HTTP方式初始化摄像头窗口
        Task.Run(async () =>
        {
            
                await InitializeCameraWindows();
                // 摄像头窗口初始化完成后启动预览
                StartPreviewAll();
                // 启用刷新监控按钮
                Dispatcher.Invoke(() => NavigationBar.EnableRefreshMonitorButton());
            
            
        });
      
    }
    
    

    /// <summary>
    /// 接收到告警推送的处理方法
    /// </summary>
    private void OnAlarmReceived(AlarmTrace alarmTrace)
    {
        
        // 在UI线程中显示通知
        Dispatcher.Invoke(() =>
        {
            Log.Information($"收到告警{alarmTrace.AlarmDate}");
            ShowAlarmNotification(alarmTrace);
        });
    }

    /// <summary>
    /// 显示告警通知窗口
    /// </summary>
    private void ShowAlarmNotification(AlarmTrace alarmTrace)
    {
       
        var notificationWindow = new AlarmNotificationWindow(alarmTrace, _alarmTraceService);
        notificationWindow.Show();

        Log.Information($"收到告警推送: {alarmTrace.AlarmType} - {alarmTrace.DeviceIp}");
    }
    
    
    
    

    /// <summary>
    /// 通过HTTP动态查询后端设备列表初始化摄像头窗口
    /// </summary>
    private async Task InitializeCameraWindows()
    {
        _cameraData = new List<CameraWindowData>();
            
        var queryDto = new DeviceQueryDto { HasChannel = true };

        var devices = await _deviceService.GetAllDevicesByQueryAsync(queryDto);
        
        devices = devices
            .Where(d => d.Enabled == 1 && !string.IsNullOrEmpty(d.Ip))
            .OrderBy(d => d.Index)
            .ToList();

            // 在UI线程中创建和添加控件
            await Dispatcher.InvokeAsync(() =>
            {
                // 创建一个LiveView控件来容纳所有摄像头
                var liveView = new LiveView(devices);

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

                Log.Information($"成功加载 {_cameraData.Count} 个摄像头");
            });
            
      
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
            await Dispatcher.InvokeAsync(() => { control.Camera = camera; });

            // 登录和启动预览可以在后台线程执行
            camera.Login();
            camera.StartPreview(channel: channel, streamType: EnumStreamType.子码流, linkMode: EnumLinkMode.RTSP);

            // 在UI线程中设置别名
            await Dispatcher.InvokeAsync(() =>
            {
                Log.Information($"摄像头 {cameraIndex + 1} - {ip}:{port} (通道{channel})");
            });
        }
        catch (Exception ex)
        {
            Log.Error("顺位{index}实时预览异常 {ErrorMessage}", cameraIndex,ex.Message);
        }
       
        
        
    }
    

    private async void StartPreviewAll()
    {
        if (_cameraData == null || _cameraData.Count == 0)
        {
            Log.Information("没有可用的摄像头数据，跳过预览启动");
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
            Log.Information("没有正在预览的摄像头");
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
                    Log.Information($"停止摄像头预览异常: {ex.Message}");
                }
            }
        }

        Log.Information("已停止所有摄像头预览");
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
                // 录像回放 - 已经在导航栏中处理
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
            Log.Information("开始刷新监控...");

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
                await InitializeCameraWindows();
                // 摄像头窗口初始化完成后启动预览
                StartPreviewAll();
            });

            Log.Information("监控刷新完成");

        }
        catch (Exception ex)
        {
            
        }
    }



    /// <summary>
    /// 初始化录像机
    /// </summary>
    private void InitializeRecorderAsync()
    {
       
            // 从appsettings.json获取RecorderIpAddress
            var recorderIpPort = Configuration["RecorderIpAddress"];

            if (string.IsNullOrEmpty(recorderIpPort))
            {
                Log.Error("配置文件中未找到RecorderIpAddress");
                return;
            }

            var ipAddress = recorderIpPort.Split(":")[0];
            var port = ushort.Parse(recorderIpPort.Split(":")[1]);

            // 创建录像机实例
            _recorder = new Recorder(ipAddress, port);
            _deviceList = new List<string>();
            _ips = new List<string>();

            // 登录设备
            var result = _recorder.Login();
            
            if (string.IsNullOrEmpty(result))
            {
                Log.Information($"录像机自动登录成功: {ipAddress}:{port}");

                // 获取关联设备列表
                LoadAssociatedDevices();
                _isRecorderInitialized = true;
            }
            else
            {
                Log.Information($"录像机自动登录失败: {result}");
            }
    }

    /// <summary>
    /// 加载关联设备列表
    /// </summary>
    private void LoadAssociatedDevices()
    {
    
        if (_recorder == null)
        {
            Log.Information("录像机实例未初始化");
            return;
        }
        
        _recorder.GetAssociatedIpList(ref _ips, ref _deviceList);
        List<RecorderItem> items = new List<RecorderItem>();
        for (int i = 0; i < _ips.Count; i++)
        {
            items.Add(new RecorderItem()
            {
                Ip = _ips[i],
                Name = _deviceList[i],
                Channel = i + 1
            });
        }
        items.Sort(((itemA, itemB) =>
        {
            try
            {
                int a = int.Parse(itemA.Name.Split("号")[0]);
                int b = int.Parse(itemB.Name.Split("号")[0]);
                return a.CompareTo(b);
            }
            catch (Exception e)
            {
                return 0;
            }
        }));
        Log.Information(string.Join(",", _ips));
        _ips = items.Select(a => a.Ip).ToList();
        
        Log.Information(string.Join(",", _deviceList));
        _deviceList = items.Select(a => a.Name).ToList();
        _channels = items.Select(a => a.Channel).ToList();
        
        
        
        
        // 调试信息：检查获取到的数据
        Log.Information($"获取到 {_ips?.Count ?? 0} 个IP地址");
        Log.Information($"获取到 {_deviceList?.Count ?? 0} 个设备名称");
        
    }

    /// <summary>
    /// 获取录像机实例
    /// </summary>
    public Recorder? GetRecorder()
    {
        return _recorder;
    }

    /// <summary>
    /// 获取设备列表
    /// </summary>
    public List<string>? GetDeviceList()
    {
        return _deviceList;
    }

    /// <summary>
    /// 获取IP列表
    /// </summary>
    public List<string>? GetIpList()
    {
        return _ips;
    }

    public List<int> GetChannelList()
    {
        return _channels;
    }

    /// <summary>
    /// 检查录像机是否已初始化
    /// </summary>
    public bool IsRecorderInitialized()
    {
        return _isRecorderInitialized;
    }
    
}