using Flurl.Http;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;

namespace RailwayMonitorClient.Services;

/// <summary>
/// 设备管理HTTP请求服务
/// </summary>
public class DeviceHttpService
{
    private readonly string _baseUrl;

    public DeviceHttpService(string baseUrl = "http://localhost:8081")
    {
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// 查询设备列表
    /// </summary>
    public async Task<BaseResponse<List<Device>>> QueryDevicesAsync(DeviceQueryDto queryDto)
    {
        return await $"{_baseUrl}/device/query"
            .PostJsonAsync(queryDto)
            .ReceiveJson<BaseResponse<List<Device>>>();
    }

    /// <summary>
    /// 获取设备通道枚举
    /// </summary>
    public async Task<BaseResponse<List<EnumResponse>>> GetDeviceChannelsAsync()
    {
        return await $"{_baseUrl}/device/query-channel"
            .GetAsync()
            .ReceiveJson<BaseResponse<List<EnumResponse>>>();
    }

    /// <summary>
    /// 获取设备类型枚举
    /// </summary>
    public async Task<BaseResponse<List<EnumResponse>>> GetDeviceTypesAsync()
    {
        return await $"{_baseUrl}/device/query-type"
            .GetAsync()
            .ReceiveJson<BaseResponse<List<EnumResponse>>>();
    }

    /// <summary>
    /// 获取设备状态枚举
    /// </summary>
    public async Task<BaseResponse<List<EnumResponse>>> GetDeviceStatusAsync()
    {
        return await $"{_baseUrl}/device/query-status"
            .GetAsync()
            .ReceiveJson<BaseResponse<List<EnumResponse>>>();
    }

    /// <summary>
    /// 创建设备
    /// </summary>
    public async Task<BaseResponse<object>> CreateDeviceAsync(DeviceCreateDto createDto)
    {
        return await $"{_baseUrl}/device/create"
            .PostJsonAsync(createDto)
            .ReceiveJson<BaseResponse<object>>();
    }

    /// <summary>
    /// 更新设备
    /// </summary>
    public async Task<BaseResponse<object>> UpdateDeviceAsync(DeviceUpdateDto updateDto)
    {
        return await $"{_baseUrl}/device/update"
            .PostJsonAsync(updateDto)
            .ReceiveJson<BaseResponse<object>>();
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    public async Task<BaseResponse<object>> DeleteDeviceAsync(int id)
    {
        return await $"{_baseUrl}/device/delete/{id}"
            .PostAsync()
            .ReceiveJson<BaseResponse<object>>();
    }

    /// <summary>
    /// 禁用设备
    /// </summary>
    public async Task<BaseResponse<object>> DisableDeviceAsync(int id)
    {
        return await $"{_baseUrl}/device/disable/{id}"
            .PostAsync()
            .ReceiveJson<BaseResponse<object>>();
    }

    /// <summary>
    /// 启用设备
    /// </summary>
    public async Task<BaseResponse<object>> EnableDeviceAsync(int id)
    {
        return await $"{_baseUrl}/device/enable/{id}"
            .PostAsync()
            .ReceiveJson<BaseResponse<object>>();
    }

    /// <summary>
    /// 测试录像机连接
    /// </summary>
    public async Task<BaseResponse<string>> TestRecorderAsync(DeviceLoginDto loginDto)
    {
        return await $"{_baseUrl}/device/test-recorder"
            .PostJsonAsync(loginDto)
            .ReceiveJson<BaseResponse<string>>();
    }

    /// <summary>
    /// 测试设备连接
    /// </summary>
    public async Task<BaseResponse<string>> TestConnectionAsync(DeviceLoginDto loginDto)
    {
        return await $"{_baseUrl}/device/test-connection"
            .PostJsonAsync(loginDto)
            .ReceiveJson<BaseResponse<string>>();
    }

    /// <summary>
    /// 测试超脑连接
    /// </summary>
    public async Task<BaseResponse<object>> TestSuperBrainAsync()
    {
        return await $"{_baseUrl}/device/test-superbrain"
            .GetAsync()
            .ReceiveJson<BaseResponse<object>>();
    }
}