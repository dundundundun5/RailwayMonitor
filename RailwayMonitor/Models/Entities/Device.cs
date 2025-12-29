using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayMonitor.Models.Entities;
[Table("device")]
public class Device
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string? Name { get; set; }
    
    [Column("index")]
    public int Index { get; set; }
    [Column("ip")]
    public string? Ip { get; set; }
    [Column("port")]
    public int Port { get; set; }
    [Column("username")]
    public string? Username { get; set; }
    [Column("password")]
    public string? Password { get; set; }
    [Column("type")]
    public int Type { get; set; }
    [Column("channel")]
    public int Channel { get; set; }
    [Column("enabled")]
    public int Enabled { get; set; } 
    [Column("create_date")]
    public DateTime CreateDate { get; set; }
    [Column("update_date")]
    public DateTime UpdateDate { get; set; }
}