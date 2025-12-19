using System.Globalization;
using System.Windows.Data;
using RailwayMonitor.Models.Enums;
using Application = System.Windows.Application;

namespace RailwayMonitor.Converters;

/// <summary>
/// 启用状态转状态文本转换器
/// </summary>
public class EnabledToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int enabled)
        {
            return enabled == (int)EnumStatus.启用 ? "启用" : "停用";
        }
        return "未知";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}

/// <summary>
/// 启用状态转颜色转换器
/// </summary>
public class EnabledToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int enabled)
        {
            return enabled == (int)EnumStatus.启用 ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
        }
        return System.Windows.Media.Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}

/// <summary>
/// 启用状态转按钮文本转换器
/// </summary>
public class EnabledToStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int enabled)
        {
            return enabled == (int)EnumStatus.启用 ? "停用" : "启用";
        }
        return "未知";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}

/// <summary>
/// 启用状态转按钮样式转换器
/// </summary>
public class EnabledToButtonStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int enabled)
        {
            return enabled == (int)EnumStatus.启用 ?
                Application.Current.FindResource("ButtonDanger") :
                Application.Current.FindResource("ButtonSuccess");
        }
        return Application.Current.FindResource("ButtonDefault");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}