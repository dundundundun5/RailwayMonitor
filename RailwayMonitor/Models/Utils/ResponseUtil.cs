using RailwayMonitor.Models.Dtos;

namespace RailwayMonitor.Models.Utils;

public static class ResponseUtil
{
    private const int SuccessCode = 200;
    private const int FailedCode = 500;

    
    
    
    public static  BaseResponse<List<T>> OfList<T>(List<T> entities)
    {
        return new BaseResponse<List<T>>()
        {
            Code = SuccessCode,
            Message = "",
            Data = entities
        };
    }

    public static BaseResponse<T> OfData<T>(T data)
    {
        return new BaseResponse<T>()
        {
            Code = SuccessCode,
            Message = "",
            Data = data
        };
    }

    public static BaseResponse<object> Failed(string message)
    {
        return new BaseResponse<object>()
        {
            Code = FailedCode,
            Message = message,
            Data = null
        };
    } 
    
    public static BaseResponse<object> Success()
    {
        return new BaseResponse<object>()
        {
            Code = SuccessCode,
            Message = "",
            Data = null
        };
    }

    public static PageResponse<T> OfPage<T>(List<T> data, long pageSize, long pageIndex, long totalCount)
    {
        return new PageResponse<T>()
        {
            Code = SuccessCode,
            Data = data,
            Message = "",
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    } 
    
    
    
}