using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;

namespace RailwayAlarmBackend.Interfaces;

public interface IAlarmTraceService
{
    Task AddAlarmTraceAsync(string deviceIp, int channel, int alarmType,string imagePath, DateTime alarmDate, string alarmModelResponse);

    Task AddAlarmTraceAsync(AlarmTrace alarmTrace);
    
    Task<Page<AlarmTrace>> GetAlarmTracePageAsync(AlarmTraceQueryDto dto);
    
    Task HandleAlarmTraceAsync(AlarmTraceHandleDto dto);
    
    Task DeleteAlarmTraceAsync(long id);

    Task PushAlarmTraceAsync(AlarmTrace alarmTrace);

}