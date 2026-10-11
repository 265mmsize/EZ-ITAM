using System.Net;

namespace EzItam.Web.Api;

/// <summary>API가 돌려준 오류. 업무 규칙 위반(400)은 화면에 메시지로 보여줍니다.</summary>
public sealed class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ApiException(HttpStatusCode statusCode, string message) : base(message) => StatusCode = statusCode;
}
