using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;

namespace RailwayAlarmBackend.Interfaces;

public interface IAlarmTraceService
{
    Task<bool> AddAlarmTraceAsync(string deviceIp, int channel, int alarmType,string imagePath, DateTime alarmDate, string alarmModelResponse);
    
    Task<PageResponse<AlarmTrace>> GetAlarmTracePageAsync(AlarmTraceQueryDto dto);
    
    Task<bool> HandleAlarmTraceAsync(AlarmTraceHandleDto dto);
    
    Task<bool> DeleteAlarmTraceAsync(long id);

}