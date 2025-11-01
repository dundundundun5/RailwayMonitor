using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Extensions.Configuration;

namespace RailwayMonitorClient.Converters;

/// <summary>
/// 通道号到点位名称转换器
/// </summary>
public class ChannelToPointConverter : IValueConverter
{
    private static int[] _superBrainChannelNames;

    static ChannelToPointConverter()
    {
        // 从appsettings.json加载超脑通道名称配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _superBrainChannelNames = configuration.GetSection("SuperBrainChannelName")
            .GetChildren()
            .Select(x => int.Parse(x.Value))
            .ToArray();
    }

    /// <summary>
    /// 将通道号转换为点位名称
    /// </summary>
    /// <param name="value">通道号</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>点位名称，如"7号点位"</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int channel && channel > 0)
        {
            if (channel >= 33)
                channel -= 32;
            // 通道号从1开始，数组索引从0开始
            int index = channel - 1;

            if (index >= 0 && index < _superBrainChannelNames.Length)
            {
                int pointNumber = _superBrainChannelNames[index];
                return $"{pointNumber}号点位";
            }
        }

        // 如果无法转换，返回原始通道号
        return $"通道{value}";
    }

    /// <summary>
    /// 反向转换（通常不需要）
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取通道对应的点位号
    /// </summary>
    /// <param name="channel">通道号</param>
    /// <returns>点位号</returns>
    public static int GetPointNumber(int channel)
    {
        if (channel >= 33)
            channel -= 32;
        if (channel > 0)
        {
            int index = channel - 1;
            if (index >= 0 && index < _superBrainChannelNames.Length)
            {
                return _superBrainChannelNames[index];
            }
        }
        return channel; // 如果无法转换，返回原始通道号
    }

    /// <summary>
    /// 获取点位号对应的通道
    /// </summary>
    /// <param name="pointNumber">点位号</param>
    /// <returns>通道号</returns>
    public static int GetChannelFromPoint(int pointNumber)
    {
        for (int i = 0; i < _superBrainChannelNames.Length; i++)
        {
            if (_superBrainChannelNames[i] == pointNumber)
            {
                return i + 1; // 数组索引+1得到通道号
            }
        }
        return pointNumber; // 如果无法转换，返回原始点位号
    }
}