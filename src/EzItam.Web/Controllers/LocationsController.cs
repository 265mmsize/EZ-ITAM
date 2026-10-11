using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

public class LocationsController : BaseController
{
    public LocationsController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : base(api, loc) { }

    public async Task<IActionResult> Index() => View(await Api.ListAsync<LocationDto>("locations"));

    [Authorize(Policy = "CanEdit")]
    public IActionResult Create() => View("Form", new LocationDto());

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationDto model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        try
        {
            await Api.CreateAsync("locations", model);
            Success(L["위치가 등록되었습니다."]);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) { AddApiError(ex); return View("Form", model); }
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id) => View("Form", await Api.GetAsync<LocationDto>("locations", id));

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LocationDto model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", model);
        try
        {
            await Api.UpdateAsync("locations", id, model);
            Success(L["위치가 저장되었습니다."]);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) { AddApiError(ex); return View("Form", model); }
    }

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await Api.DeleteAsync("locations", id);
            Success(L["위치가 삭제되었습니다."]);
        }
        catch (ApiException ex) { Error(L[ex.Message]); }
        return RedirectToAction(nameof(Index));
    }
}
