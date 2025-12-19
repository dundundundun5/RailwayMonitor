using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RailwayMonitor.Models.Dtos;

public class DeviceCreateDto
{
    [Required(ErrorMessage = "设备名不可为空")]
    [DefaultValue("第_道监控")]
    public string Name { get; set; } 
    
    public int Index { get; set; }

    [Required(ErrorMessage = "设备IP不可为空")]
    [DefaultValue("192.168.0.1")]
    public string Ip { get; set; } 

    [Required(ErrorMessage = "设备端口不可为空")]
    [DefaultValue(8000)]
    public int Port { get; set; }

    [Required(ErrorMessage = "设备用户名不可为空")]
    [DefaultValue("admin")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "设备密码不可为空")]
    [DefaultValue("114514810")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "设备类型不可为空")]
    [DefaultValue(1)] // EnumDeviceType.摄像机 的值
    public int Type { get; set; }

    [Required(ErrorMessage = "设备通道不可为空")]
    [DefaultValue(1)] // EnumChannel.通道1 的值
    public int Channel { get; set; }
}