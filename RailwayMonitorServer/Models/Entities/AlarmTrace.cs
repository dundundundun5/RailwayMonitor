using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayAlarmBackend.Models.Entities;
[Table("alarm_trace")]
public class AlarmTrace
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    [Column("device_ip")]
    public string DeviceIp { get; set; }
    [Column("super_brain_channel")]
    public int SuperBrainChannel { get; set; }
    [Column("alarm_type")]
    public int AlarmType { get; set; }
    [Column("alarm_date")]
    public DateTime AlarmDate { get; set; }
    [Column("image_path")]
    public string ImagePath { get; set; }
    [Column("alarm_status")]
    public int AlarmStatus { get; set; }
    [Column("create_date")]
    public DateTime CreateDate { get; set; }
    [Column("update_date")]
    public DateTime UpdateDate { get; set; }
}