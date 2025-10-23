using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RailwayAlarmBackend.Contexts;
using RailwayAlarmBackend.Hubs;
using RailwayAlarmBackend.Interfaces;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;
using RailwayAlarmBackend.Models.Utils;

namespace RailwayAlarmBackend.Services;

public class AlarmTraceService(DataContext context, ILogger<DeviceService> logger, IHubContext<AlarmHub> hubContext) : IAlarmTraceService
{
    public async Task<bool> AddAlarmTraceAsync(string deviceIp, int channel, int alarmType, string imagePath, DateTime alarmDate,
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
        await context.AlarmTraces.AddAsync(trace);
        await context.SaveChangesAsync();

        // 推送告警数据到WebSocket客户端
        await PushAlarmToClientsAsync(trace);

        return true;
    }

    public async Task<bool> DeleteAlarmTraceAsync(long id)
    {
        var trace = await context.AlarmTraces.FindAsync(id);
        if (trace is null)
            return false;
        context.AlarmTraces.Remove(trace);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HandleAlarmTraceAsync(AlarmTraceHandleDto dto)
    {
        var trace = await context.AlarmTraces.FindAsync(dto.Id);
        if (trace is null)
            return false;
        trace.AlarmStatus = dto.AlarmHandleStatus;
        context.AlarmTraces.Update(trace);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<PageResponse<AlarmTrace>> GetAlarmTracePageAsync(AlarmTraceQueryDto dto)
    {
        IQueryable<AlarmTrace> alarmTraces = context.AlarmTraces.OrderBy(trace => trace.Id);
        long totalCount = await alarmTraces.LongCountAsync();
        var skip = (dto.PageIndex - 1) * dto.PageSize;
        var items = await alarmTraces.Skip(skip).Take(dto.PageSize).ToListAsync();

        return PageResponseUtil.OfPage(items, dto.PageIndex, dto.PageSize, totalCount);
    }

    /// <summary>
    /// 推送告警数据到所有订阅的WebSocket客户端
    /// </summary>
    /// <param name="alarmTrace">告警数据</param>
    private async Task PushAlarmToClientsAsync(AlarmTrace alarmTrace)
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