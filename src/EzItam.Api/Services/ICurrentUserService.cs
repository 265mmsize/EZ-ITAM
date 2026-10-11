using System.Security.Claims;

namespace EzItam.Api.Services;

public interface ICurrentUserService
{
    string UserName { get; }
}

public class HttpCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    public string UserName =>
        _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
        ?? _accessor.HttpContext?.User?.Identity?.Name
        ?? "system";
}

public class StaticCurrentUserService : ICurrentUserService
{
    public StaticCurrentUserService(string userName = "system") => UserName = userName;
    public string UserName { get; }
}
