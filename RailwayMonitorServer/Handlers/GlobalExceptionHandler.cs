using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RailwayAlarmBackend.Models.Dtos;
using RailwayAlarmBackend.Models.Utils;

namespace RailwayAlarmBackend.Handlers;

/// <summary>
/// 全局异常处理类
/// 实现IExceptionHandler接口，用于捕获和处理应用程序中的所有未处理异常
/// </summary>
/// <remarks>
/// 功能特点：
/// 1. 捕获任意类型的异常
/// 2. 记录异常日志便于调试
/// 3. 统一返回ApiResult格式的错误响应
/// 4. 保持API响应格式的一致性
/// </remarks>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 处理异常
    /// </summary>
    /// <param name="httpContext">HTTP上下文</param>
    /// <param name="exception">异常对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>处理结果</returns>
    /// <remarks>
    /// 处理流程：
    /// 1. 记录异常日志
    /// 2. 使用ApiResultUtil.Failed包装异常消息
    /// 3. 设置HTTP响应状态码和内容类型
    /// 4. 返回JSON格式的错误响应
    /// </remarks>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 步骤1：记录异常日志
        // 使用结构化日志记录异常详细信息，便于后续排查问题
        _logger.LogError(exception, "全局异常捕获: {Message}", exception.Message);

        // 步骤2：使用ApiResultUtil.Failed方法包装异常消息
        // 确保错误响应格式与正常API响应格式保持一致
        var result = ResponseUtil.Failed(exception.Message);

        // 步骤3：设置HTTP响应
        // 返回500状态码表示服务器内部错误
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        // 步骤4：返回JSON格式的错误响应
        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);

        // 返回true表示异常已成功处理
        return true;
    }
}