using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RailwayMonitor.Models.Dtos;

public class AlarmTraceHandleDto
{
    [Required(ErrorMessage = "告警id不能为空")]
    [DefaultValue(1)]
    public long Id { get; set; }
    [Required(ErrorMessage = "告警处理状态不能为空")]
    [DefaultValue(1)]
    public int AlarmHandleStatus { get; set; }
}