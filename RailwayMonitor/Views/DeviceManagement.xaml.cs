using System.Windows;
using RailwayMonitorClient.Interfaces;
using RailwayMonitorClient.Models.Dtos;
using RailwayMonitorClient.Models.Entities;
using RailwayMonitorClient.Models.Enums;
using Button = System.Windows.Controls.Button;

namespace RailwayMonitorClient.Views;

public partial class DeviceManagement : HandyControl.Controls.Window
{
    private readonly IDeviceService _deviceService;

    public DeviceManagement(IDeviceService deviceService)
    {
        InitializeComponent();
        _deviceService = deviceService;
        Loaded += async (s, e) => await LoadDevicesAsync();
    }

    /// <summary>
    /// 加载设备列表
    /// </summary>
    private async Task LoadDevicesAsync()
    {
        try
        {
            TbStatus.Text = "正在加载设备列表...";

            var response = await _deviceService.GetAllDevicesByQueryAsync(new DeviceQueryDto());
            DgDevices.ItemsSource = response;
            TbStatus.Text = $"共加载 {response.Count} 个设备";
        }
        catch (Exception ex)
        {
            TbStatus.Text = $"加载失败: {ex.Message}";
            HandyControl.Controls.MessageBox.Error(ex.Message, "错误");
        }
    }

    /// <summary>
    /// 添加设备按钮点击事件
    /// </summary>
    private void BtnAddDevice_Click(object sender, RoutedEventArgs e)
    {
        // 打开添加设备窗口
        var addWindow = new DeviceEditWindow(_deviceService);
        addWindow.Closed += async (s, args) => await LoadDevicesAsync();
        addWindow.ShowDialog();
    }

    /// <summary>
    /// 刷新按钮点击事件
    /// </summary>
    private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadDevicesAsync();
    }

    /// <summary>
    /// 编辑按钮点击事件
    /// </summary>
    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int deviceId)
        {
            var device = DgDevices.Items.Cast<Device>().FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                // 打开编辑设备窗口
                var editWindow = new DeviceEditWindow(_deviceService, device);
                editWindow.Closed += async (s, args) => await LoadDevicesAsync();
                editWindow.ShowDialog();
            }
        }
    }

    /// <summary>
    /// 切换设备状态按钮点击事件
    /// </summary>
    private async void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int deviceId)
        {
            var device = DgDevices.Items.Cast<Device>().FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                try
                {
                    
                    var newStatus = device.Enabled == (int)EnumStatus.启用 ? (int)EnumStatus.停用 : (int)EnumStatus.启用;
                    
                    await _deviceService.UpdateDeviceStatusAsync(deviceId, newStatus);
                    await LoadDevicesAsync();
                    
                   
                }
                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Error(ex.Message, "错误");
                }
            }
        }
    }

    /// <summary>
    /// 删除按钮点击事件
    /// </summary>
    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int deviceId)
        {
            var device = DgDevices.Items.Cast<Device>().FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                var result = HandyControl.Controls.MessageBox.Ask($"确定要删除设备 '{device.Name}' 吗？", "确认删除");
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _deviceService.DeleteDeviceAsync(deviceId);

                        
                        await LoadDevicesAsync();
                       
                       
                    }
                    catch (Exception ex)
                    {
                        HandyControl.Controls.MessageBox.Error(ex.Message, "错误");
                    }
                }
            }
        }
    }
}