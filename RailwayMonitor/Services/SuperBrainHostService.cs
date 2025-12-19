using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RailwayMonitor.Interfaces;
using RailwayMonitor.Models.Configs;
using RailwayMonitor.Sdks;

namespace RailwayMonitor.Services;

/// <summary>
/// SuperBrain托管服务
/// 负责与超脑设备建立连接并处理告警回调
/// </summary>
public class SuperBrainHostService : BackgroundService
{
    private readonly SuperBrainConfig _config;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<SuperBrainHostService> _logger;
    private SuperBrain? _superBrain;

    public SuperBrainHostService(
        IOptions<SuperBrainConfig> config,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SuperBrainHostService> logger)
    {
        _config = config.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SuperBrainHostService 开始启动...");
        try
        {
            // 验证配置
            if (string.IsNullOrEmpty(_config.Ip))
                throw new InvalidOperationException("SuperBrain IP地址未配置");
            

            if (string.IsNullOrEmpty(_config.Port))
                throw new InvalidOperationException("SuperBrain 端口未配置");

            // 使用作用域获取IAlarmTraceService
            using var scope = _serviceScopeFactory.CreateScope();
            var alarmTraceService = scope.ServiceProvider.GetRequiredService<IAlarmTraceService>();
            
            // 创建SuperBrain实例
            _superBrain = new SuperBrain(ip: _config.Ip, port: _config.Port, username: _config.Username, password: _config.Password, alarmImageFolder: _config.ImageFolder, _config.AlarmInterval, _config.RecognizeHat,  alarmTraceService: alarmTraceService);

            _logger.LogInformation("正在连接超脑: {Ip}:{Port}", _config.Ip, _config.Port);

            // 登录超脑
            _superBrain.Login();
            _logger.LogInformation("SuperBrain登录成功");

            // _logger.LogInformation("SuperBrain透传协议-尝试获取模型描述文件");
            // string? modelInfo = _superBrain.GetModelInfo();
            // string filePath = "modelInfo.json";
            // await File.WriteAllTextAsync(filePath, modelInfo, stoppingToken);
            
            // 设置布防
            _superBrain.SetupAlarm();
            _logger.LogInformation("SuperBrain布防设置成功");

            _logger.LogInformation("SuperBrainHostService 启动完成，开始监听告警...");

            // 保持服务运行，直到收到停止信号
            // 这里使用无限等待，因为超脑会在回调中处理告警
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // 服务停止时正常退出
            _logger.LogInformation("SuperBrainHostService 收到停止信号");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SuperBrainHostService异常，服务停止");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SuperBrainHostService 正在停止...");
        try
        {
            // 清理资源
            if (_superBrain != null)
            {
                // SuperBrain类实现了IDisposable，会自动清理资源
                _superBrain.Dispose();
                _superBrain = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SuperBrainHostService 停止时发生异常");
        }

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("SuperBrainHostService 已停止");
    }
}