using System.Net;
using EzItam.Web.Api;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EzItam.Web.Infrastructure;

/// <summary>API 오류를 화면 흐름으로 바꿉니다. 폼 화면은 각 액션에서 직접 처리하고, 나머지는 여기서 처리합니다.</summary>
public sealed class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ApiException ex) return;

        switch (ex.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                context.Result = new RedirectToActionResult("Expired", "Account", null);
                break;
            case HttpStatusCode.Forbidden:
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                break;
            case HttpStatusCode.NotFound:
                context.Result = new ViewResult { ViewName = "NotFound" };
                break;
            default:
                var tempData = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(context.HttpContext);
                tempData["Alert.Error"] = ex.Message;
                var referer = context.HttpContext.Request.Headers.Referer.ToString();
                context.Result = string.IsNullOrEmpty(referer)
                    ? new RedirectToActionResult("Index", "Home", null)
                    : new RedirectResult(referer);
                break;
        }
        context.ExceptionHandled = true;
    }
}
