using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayMonitorClient.Services;

namespace RailwayMonitorClient.Views;

public partial class DeviceEditWindow : HandyControl.Controls.Window, INotifyPropertyChanged
{
    private readonly DeviceHttpService _deviceService;
    private readonly bool _isEditMode;
    private readonly Device? _originalDevice;

    public DeviceEditWindow()
    {
        InitializeComponent();
        _deviceService = new DeviceHttpService();
        _isEditMode = false;
        DataContext = this;
        Loaded += async (s, e) => await InitializeDataAsync();
    }

    public DeviceEditWindow(Device device)
    {
        InitializeComponent();
        _deviceService = new DeviceHttpService();
        _isEditMode = true;
        _originalDevice = device;
        DataContext = this;
        Loaded += async (s, e) => await InitializeDataAsync();
    }

    #region 属性

    public string WindowTitle => _isEditMode ? "编辑设备" : "添加设备";

    public string[] DeviceNameList => new[]
    {
        "01号点位",
        "02号点位",
        "03号点位",
        "04号点位",
        "05号点位",
        "06号点位",
        "07号点位",
        "08号点位",
        "09号点位",
        "10号点位",
        "11号球机"
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
            var typesResponse = await _deviceService.GetDeviceTypesAsync();
            if (typesResponse.Code == 200 && typesResponse.Data != null)
            {
                DeviceTypes = new ObservableCollection<EnumResponse>(typesResponse.Data);
            }

            // 加载通道
            var channelsResponse = await _deviceService.GetDeviceChannelsAsync();
            if (channelsResponse.Code == 200 && channelsResponse.Data != null)
            {
                Channels = new ObservableCollection<EnumResponse>(channelsResponse.Data);
            }

            // 如果是编辑模式，填充数据
            if (_isEditMode && _originalDevice != null)
            {
                SelectedDeviceName = _originalDevice.Name;
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
    private async void BtnTestConnection_Click(object sender, RoutedEventArgs e)
    { 
        if (string.IsNullOrWhiteSpace(DeviceIp) || string.IsNullOrWhiteSpace(DeviceUsername) || string.IsNullOrWhiteSpace(DevicePassword))
        {
            HandyControl.Controls.MessageBox.Warning("请填写完整的设备信息", "提示");
            return;
        }

        try
        {
            var loginDto = new DeviceLoginDto
            {
                Ip = DeviceIp,
                Port = (ushort)DevicePort,
                Username = DeviceUsername,
                Password = DevicePassword
            };

            var response = await _deviceService.TestConnectionAsync(loginDto);

            if (response.Code == 200)
            {
                HandyControl.Controls.MessageBox.Success("连接测试成功", "成功");
            }
            else
            {
                HandyControl.Controls.MessageBox.Error(response.Message, "连接失败");
            }
        }
        catch (Exception ex)
        {
            HandyControl.Controls.MessageBox.Error(ex.Message, "连接测试失败");
        }
    }

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
                var updateDto = new DeviceUpdateDto
                {
                    Id = _originalDevice.Id,
                    Name = SelectedDeviceName,
                    Ip = DeviceIp,
                    Port = DevicePort,
                    Username = DeviceUsername,
                    Password = DevicePassword,
                    Type = SelectedDeviceType,
                    Channel = SelectedChannel
                };

                var response = await _deviceService.UpdateDeviceAsync(updateDto);
                if (response.Code == 200)
                {
                    HandyControl.Controls.MessageBox.Success("设备更新成功", "成功");
                    DialogResult = true;
                    Close();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(response.Message, "更新失败");
                }
            }
            else
            {
                // 创建设备
                var createDto = new DeviceCreateDto
                {
                    Name = SelectedDeviceName,
                    Ip = DeviceIp,
                    Port = DevicePort,
                    Username = DeviceUsername,
                    Password = DevicePassword,
                    Type = SelectedDeviceType,
                    Channel = SelectedChannel
                };

                var response = await _deviceService.CreateDeviceAsync(createDto);
                if (response.Code == 200)
                {
                    HandyControl.Controls.MessageBox.Success("设备创建成功", "成功");
                    DialogResult = true;
                    Close();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(response.Message, "创建失败");
                }
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
            HandyControl.Controls.MessageBox.Warning("请选择设备名称", "提示");
            CbDeviceName.Focus();
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