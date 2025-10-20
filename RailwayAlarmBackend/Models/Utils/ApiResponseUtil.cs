using System.Collections.Generic;
using RailwayAlarmBackend.Models.Dtos;

namespace RailwayAlarmBackend.Models.Utils;

public class ApiResponseUtil
{
    public static  ApiResponse<List<T>> OfList<T>(List<T> entities)
    {
        return new ApiResponse<List<T>>()
        {
            Code = 200,
            Message = "",
            Data = entities
        };
    }

    public static ApiResponse<T> OfData<T>(T data)
    {
        return new ApiResponse<T>()
        {
            Code = 200,
            Message = "",
            Data = data
        };
    }

    public static ApiResponse<object> Failed(string message)
    {
        return new ApiResponse<object>()
        {
            Code = 500,
            Message = message,
            Data = null
        };
    } 
    
    public static ApiResponse<object> Success()
    {
        return new ApiResponse<object>()
        {
            Code = 200,
            Message = "",
            Data = null
        };
    } 
}