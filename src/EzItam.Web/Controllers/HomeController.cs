using EzItam.Web.Api;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

public class HomeController : BaseController
{
    public HomeController(ItamApiClient api) : base(api) { }

    public async Task<IActionResult> Index()
        => View(await Api.DashboardAsync());

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? code)
    {
        ViewBag.Code = code;
        ViewBag.RequestId = HttpContext.TraceIdentifier;
        return View();
    }
}
