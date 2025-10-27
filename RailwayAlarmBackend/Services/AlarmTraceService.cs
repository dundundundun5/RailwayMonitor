using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RailwayAlarmBackend.Contexts;
using RailwayAlarmBackend.Hubs;
using RailwayAlarmBackend.Interfaces;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayAlarmBackend.Services;

public class AlarmTraceService(DataContext context, ILogger<AlarmTraceService> logger, IHubContext<AlarmTraceHub> hubContext) : IAlarmTraceService
{
    public async Task AddAlarmTraceAsync(string deviceIp, int channel, int alarmType, string imagePath, DateTime alarmDate,
        string alarmModelResponse)
    {
        AlarmTrace trace = new AlarmTrace()
        {
            DeivceIp = deviceIp,
            SuperBrainChannel = channel,
            AlarmType = alarmType,
            AlarmDate = alarmDate,
            ImagePath = imagePath,
            AlarmStatus = (int)EnumAlarmStatus.未处理,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now
        };
        logger.LogInformation(alarmModelResponse);
        await AddAlarmTraceAsync(trace);
    }

    public async Task AddAlarmTraceAsync(AlarmTrace alarmTrace)
    {
        await context.AlarmTraces.AddAsync(alarmTrace);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAlarmTraceAsync(long id)
    {
        var trace = await context.AlarmTraces.FindAsync(id);
        if (trace is null)
            return;
        context.AlarmTraces.Remove(trace);
        await context.SaveChangesAsync();
    }

    public async Task HandleAlarmTraceAsync(AlarmTraceHandleDto dto)
    {
        var trace = await context.AlarmTraces.FindAsync(dto.Id);
        if (trace is null)
            return;
        trace.AlarmStatus = dto.AlarmHandleStatus;
        context.AlarmTraces.Update(trace);
        await context.SaveChangesAsync();
    }

    public async Task<Page<AlarmTrace>> GetAlarmTracePageAsync(AlarmTraceQueryDto dto)
    {
        IQueryable<AlarmTrace> alarmTraces = context.AlarmTraces.OrderBy(trace => trace.Id);
        long totalCount = await alarmTraces.LongCountAsync();
        var skip = (dto.PageIndex - 1) * dto.PageSize;
        var items = await alarmTraces.Skip(skip).Take(dto.PageSize).ToListAsync();
        Page<AlarmTrace> alarmPage = new()
        {
            Data = alarmTraces.ToList(),
            PageIndex = dto.PageIndex,
            PageSize = dto.PageSize,
            TotalCount = totalCount
        };
        return alarmPage;
    }
    
    public async Task PushAlarmTraceAsync(AlarmTrace alarmTrace)
    {
        try
        {
            logger.LogInformation("推送告警数据到WebSocket客户端: {AlarmType} - {DeviceIp}", alarmTrace.AlarmType, alarmTrace.DeivceIp);

            // 向所有订阅了告警主题的客户端推送消息
            // 消息主题为 "alarm"，内容为完整的AlarmTrace对象
            await hubContext.Clients.Group("alarm-subscribers").SendAsync("alarm", alarmTrace);

            logger.LogInformation("告警数据推送成功，告警ID: {Id}", alarmTrace.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "推送告警数据到WebSocket客户端时发生异常");
        }
    }

}