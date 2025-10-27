using Microsoft.AspNetCore.Mvc;
using RailwayAlarmBackend.Interfaces;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;
using RailwayAlarmBackend.Models.Utils;

namespace RailwayAlarmBackend.Controllers;
[ApiController]
[Route("[controller]")]
public class AlarmTraceController(IAlarmTraceService alarmTraceService) : ControllerBase
{
    [HttpPost("query")]
    public async Task<BaseResponse<Page<AlarmTrace>>> QueryAlarmTrace([FromBody] AlarmTraceQueryDto dto)
    {
        Page<AlarmTrace> alarmPage = await alarmTraceService.GetAlarmTracePageAsync(dto);
        return BaseResponseUtil.OfPage(alarmPage);
    }

    [HttpGet("query-status")]
    public async Task<BaseResponse<List<EnumResponse>>> QueryAlarmTraceHandleStatus()
    {
        List<EnumResponse> alarmStatusList = await Task.Run(EnumResponseUtil.ToList<EnumAlarmStatus>);
        return BaseResponseUtil.OfList(alarmStatusList);
    }

    [HttpPost("handle")]
    public async Task<BaseResponse<object>> HandleAlarmTrace([FromBody] AlarmTraceHandleDto dto)
    {
        await alarmTraceService.HandleAlarmTraceAsync(dto);
        return BaseResponseUtil.Success();
    }

    [HttpPost("delete/{id:long}")]
    public async Task<BaseResponse<object>> DeleteAlarmTrace([FromRoute] long id)
    {
        await alarmTraceService.DeleteAlarmTraceAsync(id);
        return BaseResponseUtil.Success();
    }
    
    
    [HttpGet("test-add-push")]
    public async Task<BaseResponse<object>> PushAddAlarm()
    {
        string deviceIp = "192.168.1.100";
        int channel = 1;
        int alarmType = 1;
       
        // 创建测试告警数据
        var testAlarm = new AlarmTrace
        {
            DeviceIp = deviceIp,
            SuperBrainChannel = channel,
            AlarmType = alarmType,
            AlarmDate = DateTime.Now,
            ImagePath = $@"./test_images/{deviceIp}_{channel}_{alarmType}.jpg",
            AlarmStatus = (int)EnumAlarmStatus.未处理,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now
        };

        await alarmTraceService.AddAlarmTraceAsync(testAlarm);
        await alarmTraceService.PushAlarmTraceAsync(testAlarm);

        return BaseResponseUtil.Success();


    }
}