using System;
using System.Globalization;
using System.Windows.Data;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayMonitorClient.Converters;

/// <summary>
/// 预警类型转换器
/// </summary>
public class AlarmTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is  int alarmType)
        {
            return alarmType switch
            {
                (int)EnumAlarmType.均穿戴 => "均穿戴",
                (int)EnumAlarmType.未戴安全帽 => "未戴安全帽",
                (int)EnumAlarmType.未穿反光衣 => "未穿反光衣",
                (int)EnumAlarmType.均未穿戴 => "均未穿戴",
                _ => "未知预警"
            };
        }

        if (value is string s)
            return s;

        return "未知预警";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}