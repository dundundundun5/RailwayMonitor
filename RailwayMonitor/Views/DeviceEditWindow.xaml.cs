using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using RailwayMonitorClient.Interfaces;
using RailwayMonitorClient.Models.Dtos;
using RailwayMonitorClient.Models.Entities;
using RailwayMonitorClient.Models.Enums;
using RailwayMonitorClient.Models.Utils;

namespace RailwayMonitorClient.Views;

public partial class DeviceEditWindow :  INotifyPropertyChanged
{
    private readonly IDeviceService _deviceService;
    private readonly bool _isEditMode;
    private readonly Device? _originalDevice;
    
    public DeviceEditWindow(IDeviceService deviceService)
    {
        InitializeComponent();
        _deviceService = deviceService;
        _isEditMode = false;

        DataContext = this;
        Loaded += async (s, e) => await InitializeDataAsync();
        if (!_isEditMode)
            Dispatcher.Invoke(() => { TbPassword.ShowEyeButton = true; });
    }

    public DeviceEditWindow(IDeviceService deviceService, Device device)
    {
        InitializeComponent();
        _deviceService = deviceService;
        _isEditMode = true;
        _originalDevice = device;
        DataContext = this;
        Loaded += async (s, e) => await InitializeDataAsync();
    }

    #region 属性

    public string WindowTitle => _isEditMode ? "编辑设备" : "添加设备";

    // 设备名称改为自由输入，不再使用预定义列表
    
    public int[] DeviceIndexList => new[]
    {
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8,
        9,
        10,
        11,
        12
    };

    private string _selectedDeviceName = "01号点位";
    public string SelectedDeviceName
    {
        get => _selectedDeviceName;
        set
        {
            _selectedDeviceName = value;
            OnPropertyChanged();
        }
    }

    private int _selectedDeviceIndex = 1;
    public int SelectedDeviceIndex
    {
        get => _selectedDeviceIndex;
        set
        {
            _selectedDeviceIndex = value;
            OnPropertyChanged();
        }
    }

    private string _deviceIp = string.Empty;
    public string DeviceIp
    {
        get => _deviceIp;
        set
        {
            _deviceIp = value;
            OnPropertyChanged();
        }
    }

    private int _devicePort = 8000;
    public int DevicePort
    {
        get => _devicePort;
        set
        {
            _devicePort = value;
            OnPropertyChanged();
        }
    }

    private string _deviceUsername = "admin";
    public string DeviceUsername
    {
        get => _deviceUsername;
        set
        {
            _deviceUsername = value;
            OnPropertyChanged();
        }
    }

    private string _devicePassword = string.Empty;
    public string DevicePassword
    {
        get => _devicePassword;
        set
        {
            _devicePassword = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<EnumResponse> _deviceTypes = new();
    public ObservableCollection<EnumResponse> DeviceTypes
    {
        get => _deviceTypes;
        set
        {
            _deviceTypes = value;
            OnPropertyChanged();
        }
    }

    private int _selectedDeviceType = 1;
    public int SelectedDeviceType
    {
        get => _selectedDeviceType;
        set
        {
            _selectedDeviceType = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<EnumResponse> _channels = new();
    public ObservableCollection<EnumResponse> Channels
    {
        get => _channels;
        set
        {
            _channels = value;
            OnPropertyChanged();
        }
    }

    private int _selectedChannel = 1;
    public int SelectedChannel
    {
        get => _selectedChannel;
        set
        {
            _selectedChannel = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region 初始化

    private async Task InitializeDataAsync()
    {
        try
        {
            // 加载设备类型
            var typesResponse = EnumResponseUtil.ToList<EnumDeviceType>();
            
            DeviceTypes = new ObservableCollection<EnumResponse>(typesResponse);
            

            // 加载通道
            var channelsResponse = EnumResponseUtil.ToList<EnumChannel>();
            
            Channels = new ObservableCollection<EnumResponse>(channelsResponse);
            

            // 如果是编辑模式，填充数据
            if (_isEditMode && _originalDevice != null)
            {
                SelectedDeviceName = _originalDevice.Name;
                SelectedDeviceIndex = _originalDevice.Index;
                DeviceIp = _originalDevice.Ip;
                DevicePort = _originalDevice.Port;
                DeviceUsername = _originalDevice.Username;
                DevicePassword = _originalDevice.Password;
                SelectedDeviceType = _originalDevice.Type;
                SelectedChannel = _originalDevice.Channel;
            }
        }
        catch (Exception ex)
        {
            HandyControl.Controls.MessageBox.Error(ex.Message, "初始化失败");
       }
    }

    #endregion

    #region 事件处理

    /// <summary>
    /// 测试连接按钮点击事件
    /// </summary>
    

    /// <summary>
    /// 保存按钮点击事件
    /// </summary>
    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInput())
            return;

        try
        {
            if (_isEditMode && _originalDevice != null)
            {
                // 更新设备
                var updateDto = new Device()
                {
                    Id = _originalDevice.Id,
                    Name = SelectedDeviceName,
                    Index = SelectedDeviceIndex,
                    Ip = DeviceIp,
                    Port = DevicePort,
                    Username = DeviceUsername,
                    Password = DevicePassword,
                    Type = SelectedDeviceType,
                    Channel = SelectedChannel
                };

                await _deviceService.UpdateDeviceAsync(updateDto);
                
                DialogResult = true;
                Close();
                
                
            }
            else
            {
                // 创建设备
                var newDevice = new Device
                {
                    Name = SelectedDeviceName,
                    Index = SelectedDeviceIndex,
                    Ip = DeviceIp,
                    Port = DevicePort,
                    Username = DeviceUsername,
                    Password = DevicePassword,
                    Type = SelectedDeviceType,
                    Channel = SelectedChannel
                };

                await _deviceService.AddDeviceAsync(newDevice);
                
                // HandyControl.Controls.MessageBox.Success("设备创建成功", "成功");
                DialogResult = true;
                Close();
               
            }
        }
        catch (Exception ex)
        {
            HandyControl.Controls.MessageBox.Error(ex.Message, "操作失败");
        }
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 验证输入
    /// </summary>
    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(SelectedDeviceName))
        {
            HandyControl.Controls.MessageBox.Warning("请输入设备名称", "提示");
            TbDeviceName.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(DeviceIp))
        {
            HandyControl.Controls.MessageBox.Warning("请输入IP地址", "提示");
            TbIp.Focus();
           return false;
        }

        if (DevicePort <= 0 || DevicePort > 65535)
        {
            HandyControl.Controls.MessageBox.Warning("请输入有效的端口号(1-65535)", "提示");
            TbPort.Focus();
            return false;
        }
 
        if (string.IsNullOrWhiteSpace(DeviceUsername))
        {
            HandyControl.Controls.MessageBox.Warning("请输入用户名", "提示");
            TbUsername.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(DevicePassword))
        {
            HandyControl.Controls.MessageBox.Warning("请输入密码", "提示");
            TbPassword.Focus();
            return false;
        }

        return true;
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}