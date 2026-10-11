using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.Extensions.Localization;

namespace EzItam.Web.Controllers;

public abstract class BaseController : Controller
{
    protected readonly ItamApiClient Api;
    protected readonly IStringLocalizer<SharedResource> L;

    protected BaseController(ItamApiClient api, IStringLocalizer<SharedResource> loc)
    {
        Api = api;
        L = loc;
    }

    protected void Success(string message) => TempData["Alert.Success"] = message;
    protected void Error(string message) => TempData["Alert.Error"] = message;

    protected static List<SelectListItem> EnumSelectList<TEnum>(TEnum? selected = null) where TEnum : struct, Enum
        => EnumDisplay.All<TEnum>()
            .Select(x => new SelectListItem(Strings.Get(x.Name), x.Value.ToString(), selected.HasValue && selected.Value.Equals(x.Value)))
            .ToList();

    protected async Task<List<SelectListItem>> OptionsAsync(string kind, string? selected = null)
        => (await Api.OptionsAsync(kind))
            .Select(o => new SelectListItem(o.Text, o.Value, o.Value == selected)).ToList();

    /// <summary>API 업무 규칙 위반을 폼 오류로 표시하기 위한 헬퍼</summary>
    protected void AddApiError(ApiException ex) => ModelState.AddModelError(string.Empty, L[ex.Message]);
}
