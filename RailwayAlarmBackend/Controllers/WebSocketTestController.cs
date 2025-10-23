using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RailwayAlarmBackend.Hubs;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;
using RailwayAlarmBackend.Models.Utils;

namespace RailwayAlarmBackend.Controllers;

/// <summary>
/// WebSocket测试控制器
/// 用于在Swagger中测试告警推送功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebSocketTestController : ControllerBase
{
    private readonly IHubContext<AlarmHub> _hubContext;
    private readonly ILogger<WebSocketTestController> _logger;

    public WebSocketTestController(
        IHubContext<AlarmHub> hubContext,
        ILogger<WebSocketTestController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// 测试推送告警消息到所有订阅的客户端
    /// </summary>
    /// <param name="deviceIp">设备IP地址</param>
    /// <param name="channel">超脑通道号</param>
    /// <param name="alarmType">告警类型 (0=均穿戴, 1=未戴安全帽, 2=未穿反光衣, 3=均未穿戴)</param>
    /// <returns>推送结果</returns>
    [HttpGet("push-alarm")]
    public async Task<BaseResponse<object>> PushTestAlarm(
         string deviceIp = "192.168.1.100",
       int channel = 1,
         int alarmType = 1)
    {
       
            // 创建测试告警数据
            var testAlarm = new AlarmTrace
            {
                DeivceIp = deviceIp,
                SuperBrainChannel = channel,
                AlarmType = alarmType,
                AlarmDate = DateTime.Now,
                ImagePath = $@"./test_images/{deviceIp}_{channel}_{alarmType}.jpg",
                AlarmStatus = (int)EnumAlarmStatus.未处理,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };

            _logger.LogInformation("开始推送测试告警: {DeviceIp} - {Channel} - {AlarmType}",
                deviceIp, channel, alarmType);

            // 推送告警到所有订阅的客户端
            await _hubContext.Clients.Group("alarm-subscribers").SendAsync("alarm", testAlarm);

            _logger.LogInformation($"测试告警推送成功 alarmData={testAlarm}");

            return BaseResponseUtil.Success();


    }
    
}