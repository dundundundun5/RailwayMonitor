using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RailwayAlarmBackend.Models.Entities;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayAlarmBackend.Models.Dtos;

public class DeviceUpdateDto
{
    [Required(ErrorMessage = "设备id不能为空")] [DefaultValue(1)]
    public int Id { get; set; }

    [DefaultValue("")] 
    public string Name { get; set; }
    [DefaultValue("")]
    public string Ip { get; set; }
    [DefaultValue(0)] 
    public int Port { get; set; }

    [DefaultValue("")] 
    public String Username { get; set; }

    [DefaultValue("")] 
    public String Password { get; set; }

    [DefaultValue((int)EnumDeviceType.摄像机)]
    public int Type { get; set; }

    [DefaultValue((int)EnumChannel.通道1)] 
    public int Channel { get; set; }
}