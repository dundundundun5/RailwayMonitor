namespace RailwayMonitorClient.Models.Dtos;

/// <summary>
/// 全局API返回结果
/// </summary>
public class BaseResponse<T>
{
    /// <summary>
    /// 状态码 (200=成功, 500=服务器错误)
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }
    
}