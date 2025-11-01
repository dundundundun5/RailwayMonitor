using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RailwayAlarmBackend.Models.Dtos;

public class DeviceLoginDto
{
    public string Ip { get; set; }
    public ushort Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}