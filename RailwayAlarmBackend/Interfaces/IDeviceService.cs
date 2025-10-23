using RailwayAlarmBackend.Models.Entities;

namespace RailwayAlarmBackend.Services;

/// <summary>
/// 设备服务接口
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// 添加设备
    /// </summary>
    /// <param name="device">设备信息</param>
    /// <returns>操作结果</returns>
    Task<bool> AddDeviceAsync(Device device);

    /// <summary>
    /// 更新设备
    /// </summary>
    /// <param name="device">设备信息</param>
    /// <returns>操作结果</returns>
    Task<bool> UpdateDeviceAsync(Device device);

    /// <summary>
    /// 获取所有设备
    /// </summary>
    /// <returns>设备列表</returns>
    Task<List<Device>> GetAllDevicesAsync();

    /// <summary>
    /// 删除设备
    /// </summary>
    /// <param name="id">设备ID</param>
    /// <returns>操作结果</returns>
    Task<bool> DeleteDeviceAsync(int id);
}