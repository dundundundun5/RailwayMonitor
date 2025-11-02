
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;
using RailwayAlarmBackend.Models.Utils;
using RailwayMonitorClient.Services;
using Button = System.Windows.Controls.Button;

namespace RailwayMonitorClient.Views;

/// <summary>
/// AlarmManagement.xaml 的交互逻辑
/// </summary>
public partial class AlarmManagement : HandyControl.Controls.Window
{
    //TODO: 处理过了就隐藏处理按钮
    private readonly AlarmTraceHttpService _alarmTraceHttpService;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount = 0;
    private int _totalPages = 0;
    public static List<EnumResponse> AlarmStatusList = EnumResponseUtil.ToList<EnumAlarmStatus>().Where((response => response.Value > 0)).ToList();
    public AlarmManagement()
    {
        InitializeComponent();
        _alarmTraceHttpService = new AlarmTraceHttpService();
        Loaded += AlarmManagement_Loaded;
    }

    private async void AlarmManagement_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAlarmData();
    }

    /// <summary>
    /// 加载预警数据
    /// </summary>
    private async Task LoadAlarmData()
    {
        try
        {
            TbStatus.Text = "正在加载数据...";

            var queryDto = new AlarmTraceQueryDto
            {
                PageIndex = _currentPage,
                PageSize = _pageSize
            };

            var response = await _alarmTraceHttpService.QueryAlarmTracesAsync(queryDto);

            if (response.Code == 200 && response.Data != null)
            {
                var alarmTraces = response.Data ?? new List<AlarmTrace>();

                // 转换图片路径并添加显示属性
                var displayAlarms = alarmTraces.Select(alarm => new DisplayAlarmTrace(alarm)).ToList();

                DgAlarms.ItemsSource = displayAlarms;

                _totalCount = (int)response.TotalCount;
                _totalPages = (int)Math.Ceiling((double)_totalCount / _pageSize);

                UpdatePageInfo();
                TbStatus.Text = $"共 {_totalCount} 条记录，当前第 {_currentPage} 页";
            }
            else
            {
                DgAlarms.ItemsSource = new List<DisplayAlarmTrace>();
                TbStatus.Text = "加载数据失败";
                System.Windows.MessageBox.Show(response.Message ?? "加载预警数据失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            TbStatus.Text = "加载数据时发生错误";
            System.Windows.MessageBox.Show($"加载预警数据时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 更新分页信息
    /// </summary>
    private void UpdatePageInfo()
    {
        TbPageInfo.Text = $"第 {_currentPage} 页 / 共 {_totalPages} 页 (共 {_totalCount} 条记录)";

        // 更新按钮状态
        BtnFirstPage.IsEnabled = _currentPage > 1;
        BtnPrevPage.IsEnabled = _currentPage > 1;
        BtnNextPage.IsEnabled = _currentPage < _totalPages;
        BtnLastPage.IsEnabled = _currentPage < _totalPages;
    }

    /// <summary>
    /// 刷新按钮点击事件
    /// </summary>
    private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadAlarmData();
    }

    /// <summary>
    /// 首页按钮点击事件
    /// </summary>
    private async void BtnFirstPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage = 1;
            await LoadAlarmData();
        }
    }

    /// <summary>
    /// 上一页按钮点击事件
    /// </summary>
    private async void BtnPrevPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            await LoadAlarmData();
        }
    }

    /// <summary>
    /// 下一页按钮点击事件
    /// </summary>
    private async void BtnNextPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            await LoadAlarmData();
        }
    }

    /// <summary>
    /// 末页按钮点击事件
    /// </summary>
    private async void BtnLastPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage = _totalPages;
            await LoadAlarmData();
        }
    }

    /// <summary>
    /// 处理按钮点击事件
    /// </summary>
    private async void BtnHandle_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.Tag is long alarmId)
        {
            // 获取当前行的ComboBox值
            var dataGridRow = FindParent<DataGridRow>(button);
            if (dataGridRow?.DataContext is DisplayAlarmTrace alarmTrace)
            {
                // 直接查找下拉框并获取选中的值
                var comboBox = FindChild<System.Windows.Controls.ComboBox>(dataGridRow, "CmbAlarmStatus");
                if (comboBox?.SelectedValue is int selectedStatus)
                {
                    try
                    {
                        var handleDto = new AlarmTraceHandleDto
                        {
                            Id = alarmId,
                            AlarmHandleStatus = selectedStatus
                        };

                        var response = await _alarmTraceHttpService.HandleAlarmTraceAsync(handleDto);

                        if (response.Code == 200)
                        {
                            // System.Windows.MessageBox.Show("处理成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadAlarmData(); // 刷新数据
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
                else
                {
                    System.Windows.MessageBox.Show("请先选择处理状态", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    /// <summary>
    /// 查看详情按钮点击事件
    /// </summary>
    private void BtnViewDetail_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.Tag is DisplayAlarmTrace alarmTrace)
        {
            // 创建详情窗口
            var detailWindow = new AlarmDetailWindow(alarmTrace);
            detailWindow.Owner = this;
            detailWindow.ShowDialog();
        }
    }

    /// <summary>
    /// 处理状态下拉框选择改变事件
    /// </summary>
    private void CmbAlarmStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.ComboBox comboBox)
        {
            if (comboBox.SelectedValue != null && (int)comboBox.SelectedValue != (int)EnumAlarmStatus.未处理)
            {
                var dataGridRow = FindParent<DataGridRow>(comboBox);
                
                // 查找处理按钮
                var handleButton = FindChild<Button>(dataGridRow, "BtnHandle");
                if (handleButton != null)
                {
                    // 直接使用绑定到AlarmStatus的值来判断
                    handleButton.IsEnabled = true;
                }
                
            }
        }
    }

    /// <summary>
    /// 查找子级元素
    /// </summary>
    private static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
    {
        if (parent == null) return null;

        T foundChild = null;
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            T childType = child as T;
            if (childType == null)
            {
                foundChild = FindChild<T>(child, childName);
                if (foundChild != null) break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                var frameworkElement = child as FrameworkElement;
                if (frameworkElement != null && frameworkElement.Name == childName)
                {
                    foundChild = (T)child;
                    break;
                }
            }
            else
            {
                foundChild = (T)child;
                break;
            }
        }

        return foundChild;
    }

    /// <summary>
    /// 查找父级元素
    /// </summary>
    private static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        if (child == null)
            return null;

        var parent = VisualTreeHelper.GetParent(child);

        if (parent == null)
            return null;

        var parentT = parent as T;
        return parentT ?? FindParent<T>(parent);
    }
}

/// <summary>
/// 用于显示的预警追踪实体
/// </summary>
public class DisplayAlarmTrace : INotifyPropertyChanged
{
    public DisplayAlarmTrace(AlarmTrace alarmTrace)
    {
        Id = alarmTrace.Id;
        DeviceIp = alarmTrace.DeviceIp;
        SuperBrainChannel = alarmTrace.SuperBrainChannel;
        AlarmType = GetAlarmTypeText(alarmTrace.AlarmType);
        AlarmDate = alarmTrace.AlarmDate;
        AlarmStatus = alarmTrace.AlarmStatus;
        AlarmStatusText = GetAlarmStatusText(alarmTrace.AlarmStatus);
        OriginalImagePath = alarmTrace.ImagePath;
        DisplayImagePath = ConvertImagePath(alarmTrace.ImagePath);
    }

    
    public long Id { get; set; }
    public string DeviceIp { get; set; }
    public int SuperBrainChannel { get; set; }
    public string AlarmType { get; set; }
    public DateTime AlarmDate { get; set; }

    private int _alarmStatus;
    public int AlarmStatus
    {
        get => _alarmStatus;
        set
        {
            if (_alarmStatus != value)
            {
                _alarmStatus = value;
                OnPropertyChanged();
                // 同时更新状态文本
                AlarmStatusText = GetAlarmStatusText(value);
            }
        }
    }

    public string AlarmStatusText { get; set; }
    public string OriginalImagePath { get; set; }
    public string DisplayImagePath { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    //TODO: 图片路径转换逻辑 OK
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

    /// <summary>
    /// 获取预警类型文本
    /// </summary>
    private static string GetAlarmTypeText(int alarmType)
    {
        return alarmType switch
        {
            0 => "均穿戴",
            1 => "未戴安全帽",
            2 => "未穿反光衣",
            3 => "均未穿戴",
            _ => "未知"
        };
    }

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