using System.Windows;
using System.Windows.Controls;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;
using RailwayMonitorClient.Services;
using Button = System.Windows.Controls.Button;

namespace RailwayMonitorClient.Views;

public partial class DeviceManagement : HandyControl.Controls.Window
{
    private readonly DeviceHttpService _deviceService;

    public DeviceManagement()
    {
        InitializeComponent();
        _deviceService = new DeviceHttpService();
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

            var response = await _deviceService.QueryDevicesAsync(new DeviceQueryDto());

            if (response.Code == 200 && response.Data != null)
            {
                DgDevices.ItemsSource = response.Data;
                TbStatus.Text = $"共加载 {response.Data.Count} 个设备";
            }
            else
            {
                TbStatus.Text = $"加载失败: {response.Message}";
                HandyControl.Controls.MessageBox.Error(response.Message, "错误");
            }
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
        var addWindow = new DeviceEditWindow();
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
                var editWindow = new DeviceEditWindow(device);
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
                    BaseResponse<object> response;

                    if (device.Enabled == (int)EnumStatus.启用)
                    {
                        // 停用设备
                        response = await _deviceService.DisableDeviceAsync(deviceId);
                    }
                    else
                    {
                        // 启用设备
                        response = await _deviceService.EnableDeviceAsync(deviceId);
                    }

                    if (response.Code == 200)
                    {
                        await LoadDevicesAsync();
                        // HandyControl.Controls.MessageBox.Success("操作成功", "成功");
                    }
                    else
                    {
                        HandyControl.Controls.MessageBox.Error(response.Message, "错误");
                    }
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
                        var response = await _deviceService.DeleteDeviceAsync(deviceId);

                        if (response.Code == 200)
                        {
                            await LoadDevicesAsync();
                            HandyControl.Controls.MessageBox.Success("删除成功", "成功");
                        }
                        else
                        {
                            HandyControl.Controls.MessageBox.Error(response.Message, "错误");
                        }
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