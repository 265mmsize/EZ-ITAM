using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

public class HomeController : BaseController
{
    public HomeController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : base(api, loc) { }

    public async Task<IActionResult> Index()
        => View(await Api.DashboardAsync());

    [AllowAnonymous]
    [HttpGet]
    public IActionResult SetLanguage(string culture, string? returnUrl = null)
    {
        var allowed = new[] { "ko-KR", "en-US", "vi-VN" };
        if (!allowed.Contains(culture)) culture = "ko-KR";
        Response.Cookies.Append(
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax });
        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? code)
    {
        ViewBag.Code = code;
        ViewBag.RequestId = HttpContext.TraceIdentifier;
        return View();
    }
}
