using Microsoft.EntityFrameworkCore;
using RailwayAlarmBackend.Contexts;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;

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
        existingDevice = newDevice;
        context.Devices.Update(existingDevice);
        await context.SaveChangesAsync();
    }
    
    public async Task<List<Device>> GetAllDevicesByQueryAsync(DeviceQueryDto dto)
    {
        var devices = await context.Devices.ToListAsync();
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