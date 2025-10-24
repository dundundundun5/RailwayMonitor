using Microsoft.AspNetCore.Mvc;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;
using RailwayAlarmBackend.Models.Utils;
using RailwayAlarmBackend.Sdks;
using RailwayAlarmBackend.Services;

namespace RailwayAlarmBackend.Controllers;
[ApiController]
[Route("[controller]")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DeviceController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpPost("query")]
    public async Task<BaseResponse<List<Device>>> QueryDevice([FromBody] DeviceQueryDto dto)
    {
        List<Device> devices = await _deviceService.GetAllDevicesByQueryAsync(dto);
        return BaseResponseUtil.OfList(devices);
    }
    
    [HttpGet("query-channel")]
    public async Task<BaseResponse<List<EnumResponse>>> QueryDeviceChannel()
    {
        List<EnumResponse> channels = await Task.Run(EnumResponseUtil.ToList<EnumChannel>); 
        return BaseResponseUtil.OfList(channels);
    }
    
    [HttpGet("query-type")]
    public async Task<BaseResponse<List<EnumResponse>>> QueryDeviceType()
    {
        List<EnumResponse> types = await Task.Run(EnumResponseUtil.ToList<EnumDeviceType>);
        return BaseResponseUtil.OfList(types);
    }

    [HttpGet("query-status")]
    public async Task<BaseResponse<List<EnumResponse>>> QueryDeviceStatus()
    {
        var statusList = await Task.Run(EnumResponseUtil.ToList<EnumStatus>);
        return BaseResponseUtil.OfList(statusList);
    }

    [HttpPost("create")]
    public async Task<BaseResponse<object>> CreateDevice([FromBody] DeviceCreateDto createDto)
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
            Enabled = (int) EnumStatus.启用
        };
        await _deviceService.AddDeviceAsync(device);
        return BaseResponseUtil.Success();
    }
    
    [HttpPost("update")]
    public async Task<BaseResponse<object>> UpdateDevice([FromBody] DeviceUpdateDto updateDto)
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
        return BaseResponseUtil.Success();
    }
    
    [HttpPost("delete/{id:int}")]
    public async Task<BaseResponse<object>> DeleteDevice([FromRoute] int id)
    {

        await _deviceService.DeleteDeviceAsync(id);
        return BaseResponseUtil.Success();
    }
    
    [HttpPost("disable/{id:int}")]
    public async Task<BaseResponse<object>> DiableDevice([FromRoute] int id)
    {

        Device newDevice = new Device()
        {
            Id = id,
            Enabled = (int)EnumStatus.停用
        };
        await _deviceService.UpdateDeviceAsync(newDevice);
        return BaseResponseUtil.Success();
    }
    
    [HttpPost("enable/{id:int}")]
    public async Task<BaseResponse<object>> EnableDevice([FromRoute] int id)
    {
        Device newDevice = new Device()
        {
            Id = id,
            Enabled = (int) EnumStatus.启用
        };
        await _deviceService.UpdateDeviceAsync(newDevice);
        return BaseResponseUtil.Success();
    }

    [HttpGet("/test-recorder")]
    public BaseResponse<object> TestRecorder()
    {
        Recorder recorder = new Recorder("192.168.122.20");
        recorder.Login();
        return BaseResponseUtil.Success();
    }
    
    [HttpGet("/test-superbrain")]
    public  BaseResponse<object> TestSuperBrain()
    {
        return BaseResponseUtil.Success();
    }
    
}