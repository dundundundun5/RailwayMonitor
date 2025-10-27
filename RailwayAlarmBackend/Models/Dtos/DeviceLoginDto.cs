using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RailwayAlarmBackend.Models.Dtos;

public class DeviceLoginDto
{
    [Required(ErrorMessage = "ip不能为空")]
    [DefaultValue("192.168.122.20")]
    public string Ip { get; set; }
    [DefaultValue(8000)]
    public ushort Port { get; set; }
    [DefaultValue("admin")]
    public string Username { get; set; }
    [DefaultValue("11111111a")]
    public string Password { get; set; }
}