namespace RailwayAlarmBackend.Models.Dtos;

public class PageResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<T>? Data { get; set; }
    public long TotalCount { get; set; }
    public long PageSize { get; set; }
    public long PageIndex { get; set; }
}