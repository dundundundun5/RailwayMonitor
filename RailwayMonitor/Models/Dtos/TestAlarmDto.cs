using Microsoft.AspNetCore.Http;

namespace RailwayMonitor.Models.Dtos;

public class TestAlarmDto
{
    public int AlarmType { get; set; }
    public IFormFile File { get; set; } = null!;
}