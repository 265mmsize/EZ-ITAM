using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.Extensions.Localization;

namespace EzItam.Web.Controllers;

public class AuditController : BaseController
{
    public AuditController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : base(api, loc) { }

    public async Task<IActionResult> Index(string? entity, string? user, DateTime? from, DateTime? to, int page = 1)
    {
        ViewBag.Entity = entity;
        ViewBag.User = user;
        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.Entities = await Api.AuditEntitiesAsync();
        return View(await Api.AuditAsync(entity, user, from, to, null, page));
    }
}
