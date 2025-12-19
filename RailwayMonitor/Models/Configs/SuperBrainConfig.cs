namespace RailwayMonitor.Models.Configs;

/// <summary>
/// 超脑配置
/// </summary>
public class SuperBrainConfig
{
    /// <summary>
    /// 超脑IP地址
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    /// 超脑端口
    /// </summary>
    public string Port { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
    
    public string ImageFolder { get; set; }
    
    public int AlarmInterval { get; set; }
    
    public bool RecognizeHat { get; set; }
}