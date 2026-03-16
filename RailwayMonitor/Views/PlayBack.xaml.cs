using System.Windows;
using System.Windows.Controls;
using RailwayMonitor.Sdks;

namespace RailwayMonitor.Views;

/// <summary>
/// PlayBack.xaml 的交互逻辑
/// </summary>
public partial class PlayBack : HandyControl.Controls.Window
{
    private Recorder? _recorder;
    private List<string> _deviceList;
    private List<string> _ips;
    private MainWindow _mainWindow;
    private bool Pause = false;
    public PlayBack(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        InitializeDateTimeControls();
        DataContext = this;
        Loaded += PlayBack_Loaded;
    }
    
    
    public string[] PlaySpeed  => 
    [
        "0.5倍速",
        "1倍速",
        "2倍速",
        "4倍速",
        "8倍速",
        "16倍速",
        "32倍速"
    ];

    /// <summary>
    /// 窗口加载完成事件
    /// </summary>
    private void PlayBack_Loaded(object sender, RoutedEventArgs e)
    {
        // 使用主窗口的录像机数据
        LoadDevicesFromMainWindow();
    }

    /// <summary>
    /// 从主窗口加载设备列表
    /// </summary>
    private void LoadDevicesFromMainWindow()
    {
        // 从主窗口获取录像机数据
        _recorder = _mainWindow.GetRecorder();
        _deviceList = _mainWindow.GetDeviceList();
        _ips = _mainWindow.GetIpList();

        if (_mainWindow.IsRecorderInitialized())
        {
            Dispatcher.Invoke(() =>
            {
                TbLoginStatus.Text = "";
                TbLoginStatus.Foreground = System.Windows.Media.Brushes.Green;
            });

            // 加载设备列表到UI
            LoadAssociatedDevices();
        }
        else
        {
            Dispatcher.Invoke(() =>
            {
                TbLoginStatus.Text = "录像机登录失败，检查录像机的IP端口";
                TbLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
            });
        }
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

    /// <summary>
    /// 开始回放按钮点击事件
    /// </summary>
    private void BtnStartPlayback_Click(object sender, RoutedEventArgs e)
    {
        if (_ips == null || _ips.Count == 0)
        {
            return;
        }

        // 检查是否有选中的设备
        if (CmbDevices.SelectedIndex < 0)
        {
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

      
        if (_recorder == null)
        {
            return;
        }

        // 获取PictureBox的句柄
        var handle = PictureBoxPlayback.Handle;

        
        // 开始回放（使用选中的通道）
        _recorder.StartPlayback(handle, startTime, endTime, channel);
        Dispatcher.Invoke(() =>
        {
            BtnPause.IsEnabled = true;
            CmbPlay.IsEnabled = true;
            BtnStartPlayback.IsEnabled = false;
        });
        
        
    }

    /// <summary>
    /// 停止回放按钮点击事件
    /// </summary>
    private void BtnStopPlayback_Click(object sender, RoutedEventArgs e)
    {
        if (_recorder == null)
        {
            return;
        }
        Dispatcher.Invoke(() =>
        {
            CmbPlay.IsEnabled = false;
            CmbPlay.SelectedIndex = 1;
            BtnPause.IsEnabled = false;
            BtnPause.Content = "暂停";
            BtnPause.Style = (System.Windows.Style)FindResource("ButtonInfo");
            BtnStartPlayback.IsEnabled = true;
        });
        _recorder.StopPlayback();
        // 清空PictureBox
        PictureBoxPlayback.Image = null;
        
            
        
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
        _recorder?.StopPlayback();

        // 清空PictureBox
        PictureBoxPlayback.Image = null;

        _recorder?.Dispose();
    }

   

    private void BtnPause_Click(object sender, RoutedEventArgs e)
    {
        if (Pause)
        {
            _recorder.Play();
            Pause = false;
            Dispatcher.Invoke(() => {
                BtnPause.Content = "暂停";
                BtnPause.Style = (System.Windows.Style)FindResource("ButtonWarning");
                CmbPlay.IsEnabled = true;
            });
            
        }
        else
        {
            _recorder.Pause();
            Pause = true;
            Dispatcher.Invoke(() => {
                BtnPause.Content = "播放";
                BtnPause.Style = (System.Windows.Style)FindResource("ButtonInfo");
                CmbPlay.IsEnabled = false;
            });
        }
    
        
    }

    
    

    private void CmbPlay_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var value = Dispatcher.Invoke(() => CmbPlay.SelectedIndex) ;
        switch (value)
        {
            case 0:
                _recorder.SlowPlay();
                break;
            case 1:
                _recorder.NormalPlay();
                break;
            case 2:
                _recorder.FastPlay(2);
                break;
            case 3:
                _recorder.FastPlay(4);
                break;
            case 4:
                _recorder.FastPlay(8);
                break;
            case 5:
                _recorder.FastPlay(16);
                break;
            case 6:
                _recorder.FastPlay(32);
                break;
            default:
                break;
            }
    }
}