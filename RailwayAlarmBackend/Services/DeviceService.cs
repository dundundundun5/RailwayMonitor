using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RailwayAlarmBackend.Contexts;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayAlarmBackend.Services;

public class DeviceService(DataContext context, ILogger<DeviceService> logger) : IDeviceService
{

    public async Task<Boolean> AddDeviceAsync(Device device)
    {
        context.Devices.Add(device);
        await context.SaveChangesAsync();
        return true;
    }
    
    public async Task<Boolean> UpdateDeviceAsync(Device newDevice)
    {
        var existingDevice = await context.Devices.FindAsync(newDevice.Id);
        if (existingDevice == null)
            throw new ArgumentException("Device not found");
        // 将newDevice的非null属性复制到existingDevice
        // DataContext的HandleEntityUpdates会自动处理null值不更新，并自动设置UpdateDate
        existingDevice = newDevice;
        context.Devices.Update(existingDevice);
        await context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<Device>> GetAllDevicesAsync()
    {
        var devices = await context.Devices.ToListAsync();
        return devices;
    }

    public async Task<Boolean> DeleteDeviceAsync(int id)
    {
        var existingDevice = await context.Devices.FindAsync(id);
        if (existingDevice == null)
            throw new Exception("设备不存在");
        context.Devices.Remove(existingDevice);
        await context.SaveChangesAsync();
        return true;
    }
}