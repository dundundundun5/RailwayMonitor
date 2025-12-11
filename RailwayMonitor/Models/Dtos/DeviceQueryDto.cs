using System.ComponentModel;

namespace RailwayMonitorClient.Models.Dtos;

public class DeviceQueryDto
{
    [DefaultValue(false)]
    public bool HasChannel { get; set; }
}