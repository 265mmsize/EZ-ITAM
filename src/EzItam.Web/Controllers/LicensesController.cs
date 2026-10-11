using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.Extensions.Localization;
using EzItam.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

public class LicensesController : BaseController
{
    public LicensesController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : base(api, loc) { }

    public async Task<IActionResult> Index(string? search, bool expiringOnly)
    {
        ViewBag.Search = search;
        ViewBag.ExpiringOnly = expiringOnly;
        return View(await Api.ListLicensesAsync(search, expiringOnly));
    }

    public async Task<IActionResult> Details(int id) => View(await Api.GetLicenseAsync(id));

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Create()
    {
        await PopulateAsync(null);
        return View(new SoftwareLicenseDto { TotalSeats = 1 });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SoftwareLicenseDto license)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var saved = await Api.CreateLicenseAsync(license);
                Success(L["라이선스가 등록되었습니다."]);
                return RedirectToAction(nameof(Details), new { id = saved.Id });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        await PopulateAsync(license);
        return View(license);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id)
    {
        var license = await Api.GetLicenseAsync(id);
        await PopulateAsync(license);
        return View(license);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SoftwareLicenseDto input)
    {
        if (id != input.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await Api.UpdateLicenseAsync(id, input);
                Success(L["라이선스 정보가 저장되었습니다."]);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        await PopulateAsync(input);
        return View(input);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await Api.DeleteLicenseAsync(id);
            Success(L["라이선스가 삭제되었습니다."]);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            Error(L[ex.Message]);
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Assign(int id)
    {
        var license = await Api.GetLicenseAsync(id);
        ViewBag.Employees = await OptionsAsync("employees");
        ViewBag.Assets = await OptionsAsync("assets");
        return View(new LicenseAssignViewModel { LicenseId = license.Id, LicenseName = license.Name });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(LicenseAssignViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await Api.AssignLicenseAsync(model.LicenseId, new LicenseAssignRequest { EmployeeId = model.EmployeeId, AssetId = model.AssetId, Notes = model.Notes });
                Success(L["라이선스가 할당되었습니다."]);
                return RedirectToAction(nameof(Details), new { id = model.LicenseId });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        ViewBag.Employees = await OptionsAsync("employees", model.EmployeeId?.ToString());
        ViewBag.Assets = await OptionsAsync("assets", model.AssetId?.ToString());
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Release(int id, int licenseId)
    {
        try
        {
            await Api.ReleaseLicenseAssignmentAsync(id);
            Success(L["라이선스 할당이 해제되었습니다."]);
        }
        catch (ApiException ex) { Error(L[ex.Message]); }
        return RedirectToAction(nameof(Details), new { id = licenseId });
    }

    private async Task PopulateAsync(SoftwareLicenseDto? license)
    {
        ViewBag.Vendors = await OptionsAsync("vendors", license?.VendorId?.ToString());
        ViewBag.Types = EnumSelectList(license?.LicenseType);
    }
}
