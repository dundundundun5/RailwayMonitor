using Microsoft.AspNetCore.SignalR;
using RailwayAlarmBackend.Models.Dtos;

namespace RailwayAlarmBackend.Hubs;

/// <summary>
/// 告警WebSocket Hub
/// 用于向前端实时推送告警数据
/// </summary>
public class AlarmHub : Hub
{
    private readonly ILogger<AlarmHub> _logger;

    public AlarmHub(ILogger<AlarmHub> logger)
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
    /// 客户端订阅告警主题
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

    /// <summary>
    /// 测试方法 - 用于验证Hub连接是否正常
    /// </summary>
    public async Task<string> TestConnection()
    {
        try
        {
            _logger.LogInformation("收到客户端 {ConnectionId} 的连接测试请求", Context.ConnectionId);
            return $"连接正常 - 客户端ID: {Context.ConnectionId}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "测试连接时发生异常");
            throw;
        }
    }
}