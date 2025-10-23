using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayAlarmBackend.Models.Entities;
[Table("alarm_trace")]
public class AlarmTrace
{
    [Column("id")]
    public long Id;
    [Column("device_ip")]
    public string DeivceIp;
    [Column("super_brain_channel")]
    public int SuperBrainChannel;
    [Column("alarm_type")]
    public int AlarmType;
    [Column("alarm_date")]
    public DateTime AlarmDate;
    [Column("image_path")]
    public string ImagePath;
    [Column("alarm_status")]
    public int AlarmStatus;
    [Column("create_date")]
    public DateTime CreateDate;
    [Column("update_date")]
    public DateTime UpdateDate;
}