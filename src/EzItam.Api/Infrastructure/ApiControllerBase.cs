using Microsoft.AspNetCore.Mvc;

namespace EzItam.Api.Infrastructure;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string CurrentUser => User.Identity?.Name ?? "system";
    protected NotFoundObjectResult NotFoundError(string what = "항목") => NotFound(new ApiError { Message = $"{what}을(를) 찾을 수 없습니다." });
}
