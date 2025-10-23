using RailwayAlarmBackend.Models.Dtos;

namespace RailwayAlarmBackend.Models.Utils;

public static class PageResponseUtil
{
    public static PageResponse<T> OfPage<T>(List<T> data, long pageIndex, long pageSize, long totalCount)
    {
        PageResponse<T> pageResponse = new PageResponse<T>()
        {
            Code = 200,
            Message = "",
            Data = data,
            PageSize = pageSize,
            PageIndex = pageIndex,
            TotalCount = totalCount
        };
        return pageResponse;
    }
}