using System.ComponentModel;

namespace RailwayMonitor.Models.Dtos;

public class DeviceQueryDto
{
    [DefaultValue(false)]
    public bool HasChannel { get; set; }
}