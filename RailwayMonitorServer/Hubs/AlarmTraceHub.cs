using Microsoft.AspNetCore.SignalR;
using RailwayAlarmBackend.Models.Dtos;

namespace RailwayAlarmBackend.Hubs;

/// <summary>
/// 告警WebSocket Hub
/// 用于向前端实时推送告警数据
/// </summary>
public class AlarmTraceHub : Hub
{
    private readonly ILogger<AlarmTraceHub> _logger;

    public AlarmTraceHub(ILogger<AlarmTraceHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 客户端连接时调用
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("客户端已连接: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 客户端断开连接时调用
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("客户端已断开: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 客户端订阅告警主题 客户端也装个SignalR包，（不支持C++），可以调用这个方法
    /// </summary>
    public async Task SubscribeToAlarms()
    {
        try
        {
            _logger.LogInformation("开始处理客户端 {ConnectionId} 的订阅请求", Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, "alarm-subscribers");

            _logger.LogInformation("客户端 {ConnectionId} 已成功订阅告警推送", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "客户端 {ConnectionId} 订阅告警推送时发生异常", Context.ConnectionId);
            throw; // 重新抛出异常，让前端知道调用失败
        }
    }

    /// <summary>
    /// 客户端取消订阅告警主题
    /// </summary>
    public async Task UnsubscribeFromAlarms()
    {
        try
        {
            _logger.LogInformation("开始处理客户端 {ConnectionId} 的取消订阅请求", Context.ConnectionId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "alarm-subscribers");

            _logger.LogInformation("客户端 {ConnectionId} 已成功取消订阅告警推送", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "客户端 {ConnectionId} 取消订阅告警推送时发生异常", Context.ConnectionId);
            throw; // 重新抛出异常，让前端知道调用失败
        }
    }
    
}