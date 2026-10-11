using EzItam.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EzItam.Api.Infrastructure;

/// <summary>업무 규칙 위반과 권한 오류를 일관된 JSON 응답으로 바꿉니다.</summary>
public sealed class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case DomainException ex:
                context.Result = new BadRequestObjectResult(new ApiError { Message = ex.Message });
                context.ExceptionHandled = true;
                break;
            case UnauthorizedAccessException ex:
                context.Result = new UnauthorizedObjectResult(new ApiError { Message = ex.Message });
                context.ExceptionHandled = true;
                break;
            default:
                _logger.LogError(context.Exception, "API 처리 중 예기치 않은 오류");
                context.Result = new ObjectResult(new ApiError { Message = "서버 내부 오류가 발생했습니다." }) { StatusCode = 500 };
                context.ExceptionHandled = true;
                break;
        }
    }
}
