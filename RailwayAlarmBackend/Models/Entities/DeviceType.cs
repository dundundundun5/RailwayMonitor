using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RailwayAlarmBackend.Models.Entities;
public class DeviceType
{
    public string? Name { get; set; }
    public int Value { get; set; }
    
}