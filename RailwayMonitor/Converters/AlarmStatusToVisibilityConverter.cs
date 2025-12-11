using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RailwayMonitorClient.Converters;

/// <summary>
/// 预警状态到可见性转换器
/// 当AlarmStatus为0（未处理）时显示，其他状态时隐藏
/// </summary>
public class AlarmStatusToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int alarmStatus)
        {
            // 当AlarmStatus为0（未处理）时显示，其他状态时隐藏
            return alarmStatus == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
        
    }
}