using RailwayMonitor.Models.Dtos;
using RailwayMonitor.Models.Entities;

namespace RailwayMonitor.Interfaces;

public interface IAlarmTraceService
{
    public event Action<AlarmTrace>? AlarmReceived;
    Task AddAlarmTraceAsync(string deviceIp, int channel, int alarmType,string imagePath, DateTime alarmDate, string alarmModelResponse);

    Task AddAlarmTraceAsync(AlarmTrace alarmTrace);
    
    Task<Page<AlarmTrace>> GetAlarmTracePageAsync(AlarmTraceQueryDto dto);
    
    Task HandleAlarmTraceAsync(AlarmTraceHandleDto dto);
    
    Task DeleteAlarmTraceAsync(long id);

    Task PushAlarmTraceAsync(AlarmTrace alarmTrace);

}