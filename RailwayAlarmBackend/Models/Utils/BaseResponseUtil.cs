using System.Collections.Generic;
using RailwayAlarmBackend.Models.Dtos;

namespace RailwayAlarmBackend.Models.Utils;

public static class BaseResponseUtil
{
    public static  BaseResponse<List<T>> OfList<T>(List<T> entities)
    {
        return new BaseResponse<List<T>>()
        {
            Code = 200,
            Message = "",
            Data = entities
        };
    }

    public static BaseResponse<T> OfData<T>(T data)
    {
        return new BaseResponse<T>()
        {
            Code = 200,
            Message = "",
            Data = data
        };
    }

    public static BaseResponse<object> Failed(string message)
    {
        return new BaseResponse<object>()
        {
            Code = 500,
            Message = message,
            Data = null
        };
    } 
    
    public static BaseResponse<object> Success()
    {
        return new BaseResponse<object>()
        {
            Code = 200,
            Message = "",
            Data = null
        };
    } 
}