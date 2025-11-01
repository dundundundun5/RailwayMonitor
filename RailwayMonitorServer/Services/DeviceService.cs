using Microsoft.EntityFrameworkCore;
using RailwayAlarmBackend.Contexts;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayAlarmBackend.Services;

public class DeviceService(DataContext context, ILogger<DeviceService> logger) : IDeviceService
{

    public async Task AddDeviceAsync(Device device)
    {
        context.Devices.Add(device);
        await context.SaveChangesAsync();
     
    }
    
    public async Task UpdateDeviceAsync(Device newDevice)
    {
        var existingDevice = await context.Devices.FindAsync(newDevice.Id);
        if (existingDevice == null)
            throw new ArgumentException("Device not found");

        // Update only the properties that are provided in newDevice
        existingDevice.Name = newDevice.Name ?? existingDevice.Name;
        existingDevice.Ip = newDevice.Ip ?? existingDevice.Ip;
        existingDevice.Port = newDevice.Port;
        existingDevice.Username = newDevice.Username ?? existingDevice.Username;
        existingDevice.Password = newDevice.Password ?? existingDevice.Password;
        existingDevice.Channel = newDevice.Channel;
        existingDevice.Type = newDevice.Type;
        existingDevice.UpdateDate = DateTime.Now;
        
        // No need to call Update() since the entity is already tracked
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateDeviceStatusAsync(int id, int status)
    {
        var existingDevice = await context.Devices.FindAsync(id);
        if (existingDevice == null)
            throw new ArgumentException("Device not found");

        // Update only the properties that are provided in newDevice
        existingDevice.Enabled = status;
        existingDevice.UpdateDate = DateTime.Now;
        
        // No need to call Update() since the entity is already tracked
        await context.SaveChangesAsync();
    }
    
    public async Task<List<Device>> GetAllDevicesByQueryAsync(DeviceQueryDto dto)
    {
        List<Device> devices;
        if (dto.HasChannel == true)
            devices = await context.Devices.Where(device => device.Channel > 0).Where(device => device.Type == (int) EnumDeviceType.摄像机 || device.Type == (int) EnumDeviceType.录像机).Where(device => device.Enabled == (int)EnumStatus.启用).ToListAsync();
        else
            devices = await context.Devices.ToListAsync();
        return devices;
    }

    public async Task DeleteDeviceAsync(int id)
    {
        var existingDevice = await context.Devices.FindAsync(id);
        if (existingDevice == null)
            throw new Exception("设备不存在");
        context.Devices.Remove(existingDevice);
        await context.SaveChangesAsync();
    }
}