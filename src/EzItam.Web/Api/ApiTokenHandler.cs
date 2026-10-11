using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace EzItam.Web.Api;

/// <summary>로그인 때 저장한 액세스 토큰을 API 호출에 붙입니다. 토큰은 브라우저에 노출하지 않습니다.</summary>
public sealed class ApiTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public ApiTokenHandler(IHttpContextAccessor accessor) => _accessor = accessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var ctx = _accessor.HttpContext;
        if (ctx is not null)
        {
            var token = await ctx.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, ct);
    }
}
