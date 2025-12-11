using System.IO;

namespace RailwayMonitorClient.Converters;

public static class PathConverter
{
    public static string ConvertToAbsoluteUri(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;

        try
        {
            // 如果已经是绝对路径，直接转换为URI
            if (Path.IsPathRooted(relativePath))
            {
                if (File.Exists(relativePath))
                {
                    return new Uri(relativePath).AbsoluteUri;
                }
                return string.Empty;
            }

            // 将相对路径转换为绝对路径
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var absolutePath = Path.Combine(baseDirectory, relativePath);

            // 确保文件存在
            if (File.Exists(absolutePath))
            {
                // 转换为file:// URI格式，WPF Image控件需要
                return new Uri(absolutePath).AbsoluteUri;
            }

            // 如果文件不存在，记录日志或返回空字符串
            return string.Empty;
        }
        catch (Exception)
        {
            // 转换失败时返回空字符串
            return string.Empty;
        }
    }
}