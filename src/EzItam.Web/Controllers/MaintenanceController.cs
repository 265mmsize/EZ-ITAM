using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

public class MaintenanceController : BaseController
{
    public MaintenanceController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : base(api, loc) { }

    public async Task<IActionResult> Index(bool openOnly, MaintenanceType? type)
    {
        ViewBag.OpenOnly = openOnly;
        ViewBag.Type = type;
        ViewBag.Types = EnumSelectList<MaintenanceType>(type);
        return View(await Api.ListMaintenanceAsync(openOnly, type));
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Create(int assetId)
    {
        var asset = await Api.GetAssetAsync(assetId);
        ViewBag.Asset = asset;
        ViewBag.Vendors = await OptionsAsync("vendors");
        ViewBag.Types = EnumSelectList<MaintenanceType>();
        return View(new MaintenanceDto { AssetId = assetId, Date = DateTime.Today });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceDto record, bool setInRepair)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await Api.CreateMaintenanceAsync(record, setInRepair);
                Success(L["유지보수 이력이 등록되었습니다."]);
                return RedirectToAction("Details", "Assets", new { id = record.AssetId });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        ViewBag.Asset = await Api.GetAssetAsync(record.AssetId);
        ViewBag.Vendors = await OptionsAsync("vendors", record.VendorId?.ToString());
        ViewBag.Types = EnumSelectList<MaintenanceType>(record.Type);
        return View(record);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id)
    {
        var record = await Api.GetMaintenanceAsync(id);
        ViewBag.Asset = await Api.GetAssetAsync(record.AssetId);
        ViewBag.Vendors = await OptionsAsync("vendors", record.VendorId?.ToString());
        ViewBag.Types = EnumSelectList<MaintenanceType>(record.Type);
        return View(record);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MaintenanceDto input, bool restoreToStock)
    {
        if (id != input.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                var saved = await Api.UpdateMaintenanceAsync(id, input, restoreToStock);
                Success(L["유지보수 이력이 저장되었습니다."]);
                return RedirectToAction("Details", "Assets", new { id = saved.AssetId });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        ViewBag.Asset = await Api.GetAssetAsync(input.AssetId);
        ViewBag.Vendors = await OptionsAsync("vendors", input.VendorId?.ToString());
        ViewBag.Types = EnumSelectList<MaintenanceType>(input.Type);
        return View(input);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await Api.GetMaintenanceAsync(id);
        await Api.DeleteMaintenanceAsync(id);
        Success(L["유지보수 이력이 삭제되었습니다."]);
        return RedirectToAction("Details", "Assets", new { id = record.AssetId });
    }
}
