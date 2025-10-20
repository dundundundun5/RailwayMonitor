using Microsoft.AspNetCore.Mvc;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;
using RailwayAlarmBackend.Models.Utils;
using RailwayAlarmBackend.Services;

namespace RailwayAlarmBackend.Controllers;
[ApiController]
[Route("[controller]")]
public class DeviceController : ControllerBase
{
    private readonly DeviceService _deviceService;

    public DeviceController(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet("query")]
    public async Task<ApiResponse<List<Device>>> QueryDevice()
    {
        List<Device> devices = await _deviceService.GetAllDevicesAsync();
        return ApiResponseUtil.OfList(devices);
    }
    
    [HttpGet("query-channel")]
    public async Task<ApiResponse<List<Channel>>> QueryDeviceChannel()
    {
        List<Channel> channels = Enum
            .GetValues<EnumChannel>()
            .Select(e => (new Channel { Name = e.ToString(), Value = (int)e }))
            .ToList();
        return ApiResponseUtil.OfList(channels);
    }
    
    [HttpGet("query-type")]
    public async Task<ApiResponse<List<DeviceType>>> QueryDeviceType()
    {
        List<DeviceType> types = Enum
            .GetValues<EnumChannel>()
            .Select(e => (new DeviceType { Name = e.ToString(), Value = (int)e }))
            .ToList();
        return ApiResponseUtil.OfList(types);
    }

    [HttpPost("create")]
    public async Task<ApiResponse<object>> CreateDevice([FromBody] DeviceCreateDto createDto)
    {
        Device device = new Device
        {
            Name = createDto.Name,
            Ip = createDto.Ip,
            Port = createDto.Port,
            Username = createDto.Username,
            Password = createDto.Password,
            Channel = createDto.Channel,
            Type = createDto.Type,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            Enabled = (int) EnumStatus.Enabled
        };
        await _deviceService.AddDeviceAsync(device);
        return ApiResponseUtil.Success();
    }
    
    [HttpPost("update")]
    public async Task<ApiResponse<object>> UpdateDevice([FromBody] DeviceUpdateDto updateDto)
    {
        Device device = new Device
        {
            Name = updateDto.Name,
            Ip = updateDto.Ip,
            Port = updateDto.Port,
            Username = updateDto.Username,
            Password = updateDto.Password,
            Channel = updateDto.Channel,
            Type = updateDto.Type,
            // CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            // Enabled = (int) EnumStatus.Enabled
        };
        await _deviceService.UpdateDeviceAsync(device);
        return ApiResponseUtil.Success();
    }
    
    [HttpPost("delete/{id:int}")]
    public async Task<ApiResponse<object>> DeleteDevice([FromRoute] int id)
    {

        await _deviceService.DeleteDeviceAsync(id);
        return ApiResponseUtil.Success();
    }
    
    [HttpPost("disable/{id:int}")]
    public async Task<ApiResponse<object>> DiableDevice([FromRoute] int id)
    {

        Device newDevice = new Device()
        {
            Id = id,
            Enabled = (int)EnumStatus.Disabled
        };
        await _deviceService.UpdateDeviceAsync(newDevice);
        return ApiResponseUtil.Success();
    }
    
    [HttpPost("enable/{id:int}")]
    public async Task<ApiResponse<object>> EnableDevice([FromRoute] int id)
    {
        Device newDevice = new Device()
        {
            Id = id,
            Enabled = (int) EnumStatus.Enabled
        };
        await _deviceService.UpdateDeviceAsync(newDevice);
        return ApiResponseUtil.Success();
    }
    
}