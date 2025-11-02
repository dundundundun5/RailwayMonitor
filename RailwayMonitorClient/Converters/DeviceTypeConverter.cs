using System;
using System.Globalization;
using System.Windows.Data;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayMonitorClient.Converters;

/// <summary>
/// 设备类型转换器
/// </summary>
public class DeviceTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int deviceType)
        {
            return deviceType switch
            {
                (int)EnumDeviceType.摄像机 => "摄像机",
                (int)EnumDeviceType.录像机 => "录像机",
                (int)EnumDeviceType.超脑 => "超脑",
                _ => "未知设备"
            };
        }

        return "未知设备";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}