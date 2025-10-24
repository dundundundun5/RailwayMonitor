using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Entities;

namespace RailwayAlarmBackend.Models.Utils;

public static class BaseResponseUtil
{
    private const int SuccessCode = 200;
    private const int FailedCode = 500;

    public static BaseResponse<Page<T>> OfPage<T>(Page<T> entities)
    {
        return new BaseResponse<Page<T>>()
        {
            Code = SuccessCode,
            Message = "",
            Data = entities
        };
    }
    
    
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
    
}