using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using RailwayAlarmBackend.Models.Entities;

namespace RailwayMonitorClient.Services;

/// <summary>
/// SignalR告警推送服务
/// </summary>
public class AlarmHubService
{
    private HubConnection _hubConnection;
    private readonly string _baseUrl;

    public event Action<AlarmTrace> OnAlarmReceived;

    public AlarmHubService(string baseUrl = "http://localhost:8081")
    {
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// 启动SignalR连接
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{_baseUrl}/alarmHub")
                .WithAutomaticReconnect()
                .Build();

            // 注册接收告警的方法
            _hubConnection.On<AlarmTrace>("alarm", (alarmTrace) =>
            {
                OnAlarmReceived?.Invoke(alarmTrace);
            });

            // 连接状态变化事件
            _hubConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _hubConnection.StartAsync();
            };

            // 启动连接
            await _hubConnection.StartAsync();

            // 订阅告警推送
            await _hubConnection.InvokeAsync("SubscribeToAlarms");

            Console.WriteLine("SignalR连接已建立并订阅告警推送");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR连接失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 停止SignalR连接
    /// </summary>
    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }

    /// <summary>
    /// 检查连接状态
    /// </summary>
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    /// <summary>
    /// 重新连接
    /// </summary>
    public async Task ReconnectAsync()
    {
        await StopAsync();
        await StartAsync();
    }
}