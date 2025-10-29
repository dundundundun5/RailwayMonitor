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
public class DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger) : ControllerBase
{
    [HttpPost("query")]
    public async Task<BaseResponse<List<Device>>> QueryDevice([FromBody] DeviceQueryDto dto)
    {
        List<Device> devices = await deviceService.GetAllDevicesByQueryAsync(dto);
        return ResponseUtil.OfList(devices);
    }
    
    [HttpGet("query-channel")]
    public async Task<BaseResponse<List<EnumResponse>>> QueryDeviceChannel()
    {
        List<EnumResponse> channels = await Task.Run(EnumResponseUtil.ToList<EnumChannel>); 
        return ResponseUtil.OfList(channels);
    }
    
    [HttpGet("query-type")]
    public async Task<BaseResponse<List<EnumResponse>>> QueryDeviceType()
    {
        List<EnumResponse> types = await Task.Run(EnumResponseUtil.ToList<EnumDeviceType>);
        return ResponseUtil.OfList(types);
    }

    [HttpGet("query-status")]
    public async Task<BaseResponse<List<EnumResponse>>> QueryDeviceStatus()
    {
        var statusList = await Task.Run(EnumResponseUtil.ToList<EnumStatus>);
        return ResponseUtil.OfList(statusList);
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
        await deviceService.AddDeviceAsync(device);
        return ResponseUtil.Success();
    }
    
    [HttpPost("update")]
    public async Task<BaseResponse<object>> UpdateDevice([FromBody] DeviceUpdateDto updateDto)
    {
        Device device = new Device
        {
            Id = updateDto.Id,
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
        await deviceService.UpdateDeviceAsync(device);
        return ResponseUtil.Success();
    }
    
    [HttpPost("delete/{id:int}")]
    public async Task<BaseResponse<object>> DeleteDevice([FromRoute] int id)
    {

        await deviceService.DeleteDeviceAsync(id);
        return ResponseUtil.Success();
    }
    
    [HttpPost("disable/{id:int}")]
    public async Task<BaseResponse<object>> DiableDevice([FromRoute] int id)
    {

        await deviceService.UpdateDeviceStatusAsync(id, (int)EnumStatus.停用);
        return ResponseUtil.Success();
    }
    
    [HttpPost("enable/{id:int}")]
    public async Task<BaseResponse<object>> EnableDevice([FromRoute] int id)
    {
    
        await deviceService.UpdateDeviceStatusAsync(id, (int)EnumStatus.启用);
        return ResponseUtil.Success();
    }

    [HttpPost("test-recorder")]
    public BaseResponse<string> TestRecorder([FromBody] DeviceLoginDto dto)
    {
        logger.LogInformation("TestRecorder Login");
        Recorder recorder = new Recorder(dto.Ip, dto.Port, dto.Username, dto.Password);
        recorder.Login();
        List<(string, string)> associatedIpList = recorder.GetAssociatedIpList();
        string ips = string.Join("", associatedIpList);
        string result = $"{recorder.Ip}接了{associatedIpList.Count}路={ips}";
        logger.LogInformation(result);
        return ResponseUtil.OfData(result);
    }
    
    [HttpPost("test-connection")]
    public BaseResponse<string> TestLogin([FromBody] DeviceLoginDto dto)
    {
        logger.LogInformation("TestRecorder Login");
        Recorder recorder = new Recorder(dto.Ip, dto.Port, dto.Username, dto.Password);
        string result =  recorder.Login();
        return ResponseUtil.OfData(result);
    }
    
    [HttpGet("test-superbrain")]
    public async Task<BaseResponse<object>> TestSuperBrain()
    {
        return ResponseUtil.Success();
    }
    
}