using System.ComponentModel;

namespace RailwayAlarmBackend.Models.Dtos;

public class DeviceQueryDto
{
    [DefaultValue(false)]
    public bool HasChannel { get; set; }
}