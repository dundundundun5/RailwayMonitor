using RailwayMonitor.Models.Dtos;
using RailwayMonitor.Models.Entities;

namespace RailwayMonitor.Interfaces;

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
    Task AddDeviceAsync(Device device);

    /// <summary>
    /// 更新设备
    /// </summary>
    /// <param name="device">设备信息</param>
    /// <returns>操作结果</returns>
    Task UpdateDeviceAsync(Device device);

    public Task UpdateDeviceStatusAsync(int id, int status);

    /// <summary>
    /// 获取所有设备
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>设备列表</returns>
    Task<List<Device>> GetAllDevicesByQueryAsync(DeviceQueryDto dto);
    

    /// <summary>
    /// 删除设备
    /// </summary>
    /// <param name="id">设备ID</param>
    /// <returns>操作结果</returns>
    Task DeleteDeviceAsync(int id);
    
    
}