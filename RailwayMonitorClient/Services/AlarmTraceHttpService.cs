using Flurl.Http;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;

namespace RailwayMonitorClient.Services;

/// <summary>
/// 预警管理HTTP请求服务
/// </summary>
public class AlarmTraceHttpService
{
    private readonly string _baseUrl;

    public AlarmTraceHttpService(string baseUrl = "http://localhost:8081")
    {
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// 查询预警记录
    /// </summary>
    public async Task<PageResponse<AlarmTrace>> QueryAlarmTracesAsync(AlarmTraceQueryDto queryDto)
    {
        return await $"{_baseUrl}/alarmtrace/query"
            .PostJsonAsync(queryDto)
            .ReceiveJson<PageResponse<AlarmTrace>>();
    }

    /// <summary>
    /// 获取预警状态枚举
    /// </summary>
    public async Task<BaseResponse<List<EnumResponse>>> GetAlarmStatusAsync()
    {
        return await $"{_baseUrl}/alarmtrace/query-status"
            .GetAsync()
            .ReceiveJson<BaseResponse<List<EnumResponse>>>();
    }

    /// <summary>
    /// 获取预警类型枚举
    /// </summary>
    public async Task<BaseResponse<List<EnumResponse>>> GetAlarmTypesAsync()
    {
        return await $"{_baseUrl}/alarmtrace/query-type"
            .GetAsync()
            .ReceiveJson<BaseResponse<List<EnumResponse>>>();
    }

    /// <summary>
    /// 处理预警
    /// </summary>
    public async Task<BaseResponse<object>> HandleAlarmTraceAsync(AlarmTraceHandleDto handleDto)
    {
        return await $"{_baseUrl}/alarmtrace/handle"
            .PostJsonAsync(handleDto)
            .ReceiveJson<BaseResponse<object>>();
    }

    /// <summary>
    /// 删除预警
    /// </summary>
    public async Task<BaseResponse<object>> DeleteAlarmTraceAsync(long id)
    {
        return await $"{_baseUrl}/alarmtrace/delete/{id}"
            .PostAsync()
            .ReceiveJson<BaseResponse<object>>();
    }

    /// <summary>
    /// 测试推送预警
    /// </summary>
    public async Task<BaseResponse<object>> TestPushAlarmAsync()
    {
        return await $"{_baseUrl}/alarmtrace/test-add-push"
            .GetAsync()
            .ReceiveJson<BaseResponse<object>>();
    }

    /// <summary>
    /// 测试上传图片预警
    /// </summary>
    public async Task<BaseResponse<object>> TestAlarmWithFileAsync(TestAlarmDto testAlarmDto)
    {
        return await $"{_baseUrl}/alarmtrace/test-alarm"
            .PostMultipartAsync(mp =>
            {
                mp.AddFile("file", testAlarmDto.FilePath);
                mp.AddString("alarmType", testAlarmDto.AlarmType.ToString());
            })
            .ReceiveJson<BaseResponse<object>>();
    }
}

/// <summary>
/// 测试预警DTO（用于文件上传）
/// </summary>
public class TestAlarmDto
{
    public int AlarmType { get; set; }
    public string FilePath { get; set; } = string.Empty;
}