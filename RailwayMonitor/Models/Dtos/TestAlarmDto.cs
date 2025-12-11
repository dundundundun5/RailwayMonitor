using Microsoft.AspNetCore.Http;

namespace RailwayMonitorClient.Models.Dtos;

public class TestAlarmDto
{
    public int AlarmType { get; set; }
    public IFormFile File { get; set; } = null!;
}