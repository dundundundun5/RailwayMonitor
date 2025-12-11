
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RailwayMonitorClient.Contexts;
using RailwayMonitorClient.Interfaces;
using RailwayMonitorClient.Models.Dtos;
using RailwayMonitorClient.Models.Entities;
using RailwayMonitorClient.Models.Enums;

namespace RailwayMonitorClient.Services;

public class AlarmTraceService(DataContext context, ILogger<AlarmTraceService> logger) : IAlarmTraceService
{
    public event Action<AlarmTrace> AlarmReceived = trace =>
    {
        
    };
    private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1); // 数据库操作锁
    public async Task AddAlarmTraceAsync(string deviceIp, int channel, int alarmType, string imagePath, DateTime alarmDate,
        string alarmModelResponse)
    {
        AlarmTrace trace = new AlarmTrace()
        {
            DeviceIp = deviceIp,
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
        await _dbSemaphore.WaitAsync();
        try
        {
            await context.AlarmTraces.AddAsync(alarmTrace);
            await context.SaveChangesAsync();

            // 推送告警数据到WebSocket
            await PushAlarmTraceAsync(alarmTrace);
        }
        finally
        {
            _dbSemaphore.Release();
        }
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
        
        await context.SaveChangesAsync();
    }

    public async Task<Page<AlarmTrace>> GetAlarmTracePageAsync(AlarmTraceQueryDto dto)
    {
        //TODO: 分页倒序查询
        IQueryable<AlarmTrace> alarmTraces;
        if (dto.Ascending)
            alarmTraces = context.AlarmTraces.OrderBy(trace => trace.Id);
        else 
            alarmTraces = context.AlarmTraces.OrderByDescending(trace => trace.Id);
        
        long totalCount = await alarmTraces.LongCountAsync();
        var skip = (dto.PageIndex - 1) * dto.PageSize;
        var items = await alarmTraces.Skip(skip).Take(dto.PageSize).ToListAsync();
        Page<AlarmTrace> alarmPage = new()
        {
            //TODO: 分页返回翻页后的数据
            Data = items,
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
            logger.LogInformation("推送告警数据到WebSocket客户端: {AlarmType} - {DeviceIp}", alarmTrace.AlarmType, alarmTrace.DeviceIp);
            
            AlarmReceived?.Invoke(alarmTrace);

            logger.LogInformation("告警数据推送成功，告警ID: {Id}", alarmTrace.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "推送告警数据到WebSocket客户端时发生异常");
        }
    }

}