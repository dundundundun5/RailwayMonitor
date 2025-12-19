using RailwayMonitor.Models.Entities;

namespace RailwayMonitor.Models.Dtos;

public class PageResponse<T> : Page<T>
{
    public int Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
}