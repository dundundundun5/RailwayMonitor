namespace RailwayMonitorClient.Models.Entities;

public class Page<T>
{
    public List<T> Data { get; set; }
    public long TotalCount { get; set; }
    public long PageSize { get; set; }
    public long PageIndex { get; set; }
}