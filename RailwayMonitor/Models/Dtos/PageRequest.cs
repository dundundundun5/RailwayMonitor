namespace RailwayMonitor.Models.Dtos;

public class PageRequest
{
    public int PageSize { get; set; } = 20;
    public int PageIndex { get; set; } = 1;
    public bool Ascending { get; set; }= false;
    public bool Pageable { get; set; } = true;
}