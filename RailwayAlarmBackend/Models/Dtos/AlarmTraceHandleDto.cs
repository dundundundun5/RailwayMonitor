using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayAlarmBackend.Models.Dtos;

public class AlarmTraceHandleDto
{
    [Required(ErrorMessage = "告警id不能为空")]
    [DefaultValue(1)]
    public int Id { get; set; }
    [Required(ErrorMessage = "告警处理状态不能为空")]
    [DefaultValue(1)]
    public int AlarmHandleStatus { get; set; }
}