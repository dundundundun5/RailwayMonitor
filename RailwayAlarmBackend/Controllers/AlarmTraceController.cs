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
    public async Task<PageResponse<AlarmTrace>> QueryAlarmTrace([FromBody] AlarmTraceQueryDto dto)
    {
        Page<AlarmTrace> alarmPage = await alarmTraceService.GetAlarmTracePageAsync(dto);
        return ResponseUtil.OfPage(alarmPage.Data, alarmPage.PageSize, alarmPage.PageIndex, alarmPage.TotalCount);
    }

    [HttpGet("query-status")]
    public async Task<BaseResponse<List<EnumResponse>>> QueryAlarmTraceHandleStatus()
    {
        List<EnumResponse> alarmStatusList = await Task.Run(EnumResponseUtil.ToList<EnumAlarmStatus>);
        return ResponseUtil.OfList(alarmStatusList);
    }
    
    [HttpGet("query-type")]
    public async Task<BaseResponse<List<EnumResponse>>> QueryAlarmTraceType()
    {
        List<EnumResponse> alarmTypeList = await Task.Run(EnumResponseUtil.ToList<EnumAlarmType>);
        return ResponseUtil.OfList(alarmTypeList);
    }

    [HttpPost("handle")]
    public async Task<BaseResponse<object>> HandleAlarmTrace([FromBody] AlarmTraceHandleDto dto)
    {
        await alarmTraceService.HandleAlarmTraceAsync(dto);
        return ResponseUtil.Success();
    }

    [HttpPost("delete/{id:long}")]
    public async Task<BaseResponse<object>> DeleteAlarmTrace([FromRoute] long id)
    {
        await alarmTraceService.DeleteAlarmTraceAsync(id);
        return ResponseUtil.Success();
    }
    
    
    [HttpGet("test-add-push")]
    public async Task<BaseResponse<object>> PushAddAlarm()
    {
        string deviceIp = "192.168.100.100";
        int channel = 1;
        int alarmType = 1;
       
        // 创建测试告警数据
        var testAlarm = new AlarmTrace
        {
            DeviceIp = deviceIp,
            SuperBrainChannel = channel,
            AlarmType = alarmType,
            AlarmDate = DateTime.Now,
            ImagePath = $@"./alarmTraceImage/laoda.jpg",
            AlarmStatus = (int)EnumAlarmStatus.未处理,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now
        };

        // await alarmTraceService.AddAlarmTraceAsync(testAlarm);
        await alarmTraceService.PushAlarmTraceAsync(testAlarm);

        return ResponseUtil.Success();


    }
    
    [HttpPost("test-alarm")]
    public async Task<BaseResponse<object>> TestAlarmWithFile([FromForm] TestAlarmDto dto)
    {
        // 验证文件类型
        if (dto.File == null || dto.File.Length == 0)
        {
            return ResponseUtil.Failed("请上传有效的图片文件");
        }

        // 验证文件类型为jpg或png
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return ResponseUtil.Failed("只支持jpg和png格式的图片");
        }
    
        // 确保alarmTraceImage目录存在
        var alarmImageFolder = "alarmTraceImage";
        if (!Directory.Exists(alarmImageFolder))
        {
            Directory.CreateDirectory(alarmImageFolder);
        }
    
        // 生成唯一文件名
        var fileName = $"test_alarm_{DateTime.Now:yyyyMMddHHmmssfff}{fileExtension}";
        var filePath = Path.Combine(alarmImageFolder, fileName);
    
        // 保存文件
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        // 创建告警数据
        string deviceIp = "192.168.100.100";
        int channel = 1;

        var testAlarm = new AlarmTrace
        {
            DeviceIp = deviceIp,
            SuperBrainChannel = channel,
            AlarmType = dto.AlarmType,
            AlarmDate = DateTime.Now,
            ImagePath = $@"./{filePath}",
            AlarmStatus = (int)EnumAlarmStatus.未处理,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now
        };
    
        await alarmTraceService.AddAlarmTraceAsync(testAlarm);
        await alarmTraceService.PushAlarmTraceAsync(testAlarm);
    
        return ResponseUtil.Success();
    }
}