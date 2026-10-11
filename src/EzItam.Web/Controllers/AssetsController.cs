using EzItam.Web.Api;
using EzItam.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

public class AssetsController : BaseController
{
    public AssetsController(ItamApiClient api) : base(api) { }

    public async Task<IActionResult> Index(AssetFilter filter)
    {
        var result = await Api.SearchAssetsAsync(filter);
        ViewBag.Categories = await OptionsAsync("categories", filter.CategoryId?.ToString());
        ViewBag.Locations = await OptionsAsync("locations", filter.LocationId?.ToString());
        ViewBag.Departments = await OptionsAsync("departments", filter.DepartmentId?.ToString());
        ViewBag.Statuses = EnumSelectList(filter.Status);
        return View(new AssetIndexViewModel { Filter = filter, Result = result });
    }

    public async Task<IActionResult> Export(AssetFilter filter)
    {
        var bytes = await Api.ExportAssetsAsync(filter);
        return File(bytes, "text/csv; charset=utf-8", $"assets_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    public async Task<IActionResult> Details(int id)
    {
        var asset = await Api.GetAssetAsync(id);
        var logs = await Api.AuditAsync("Asset", null, null, null, id.ToString(), 1);
        ViewBag.AuditLogs = logs.Items.Take(20).ToList();
        return View(asset);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Create(int? categoryId)
    {
        var asset = new AssetDto { Status = AssetStatus.InStock, CategoryId = categoryId ?? 0 };
        if (categoryId.HasValue)
        {
            try { asset.AssetTag = await Api.NextTagAsync(categoryId.Value); }
            catch (ApiException) { }
        }
        await PopulateFormListsAsync(asset);
        return View(asset);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetDto asset)
    {
        if (asset.CategoryId == 0) ModelState.AddModelError(nameof(AssetDto.CategoryId), "분류를 선택하세요.");
        if (ModelState.IsValid)
        {
            try
            {
                var saved = await Api.CreateAssetAsync(asset);
                Success($"자산 {saved.AssetTag} 이(가) 등록되었습니다.");
                return RedirectToAction(nameof(Details), new { id = saved.Id });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        await PopulateFormListsAsync(asset);
        return View(asset);
    }

    [HttpGet]
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> NextTag(int categoryId)
    {
        try { return Json(new { tag = await Api.NextTagAsync(categoryId) }); }
        catch (ApiException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id)
    {
        var asset = await Api.GetAssetAsync(id);
        await PopulateFormListsAsync(asset);
        return View(asset);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AssetDto asset)
    {
        if (id != asset.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await Api.UpdateAssetAsync(id, asset);
                Success("자산 정보가 저장되었습니다.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        await PopulateFormListsAsync(asset);
        return View(asset);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await Api.DeleteAssetAsync(id);
            Success("자산이 삭제되었습니다.");
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            Error(ex.Message);
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> CheckOut(int id)
    {
        var asset = await Api.GetAssetAsync(id);
        ViewBag.Employees = await OptionsAsync("employees");
        return View(new CheckOutViewModel { AssetId = asset.Id, AssetTag = asset.AssetTag, AssetName = asset.Name });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(CheckOutViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await Api.CheckOutAsync(model.AssetId, new CheckOutRequest { EmployeeId = model.EmployeeId!.Value, Notes = model.Notes });
                Success("불출 처리되었습니다.");
                return RedirectToAction(nameof(Details), new { id = model.AssetId });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        ViewBag.Employees = await OptionsAsync("employees", model.EmployeeId?.ToString());
        return View(model);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> CheckIn(int id)
    {
        var asset = await Api.GetAssetAsync(id);
        ViewBag.Locations = await OptionsAsync("locations", asset.LocationId?.ToString());
        ViewBag.Statuses = CheckInStatuses();
        return View(new CheckInViewModel
        {
            AssetId = asset.Id, AssetTag = asset.AssetTag, AssetName = asset.Name,
            CurrentEmployeeName = asset.CurrentEmployee?.DisplayName, LocationId = asset.LocationId
        });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await Api.CheckInAsync(model.AssetId, new CheckInRequest { NewStatus = model.NewStatus, LocationId = model.LocationId, Notes = model.Notes });
                Success("반납 처리되었습니다.");
                return RedirectToAction(nameof(Details), new { id = model.AssetId });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        ViewBag.Locations = await OptionsAsync("locations", model.LocationId?.ToString());
        ViewBag.Statuses = CheckInStatuses();
        return View(model);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> ChangeStatus(int id)
    {
        var asset = await Api.GetAssetAsync(id);
        ViewBag.Statuses = CheckInStatuses();
        return View(new ChangeStatusViewModel { AssetId = asset.Id, AssetTag = asset.AssetTag, AssetName = asset.Name, CurrentStatus = asset.Status, NewStatus = asset.Status });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(ChangeStatusViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await Api.ChangeStatusAsync(model.AssetId, new ChangeStatusRequest { NewStatus = model.NewStatus, Notes = model.Notes });
                Success("상태가 변경되었습니다.");
                return RedirectToAction(nameof(Details), new { id = model.AssetId });
            }
            catch (ApiException ex) { AddApiError(ex); }
        }
        ViewBag.Statuses = CheckInStatuses();
        return View(model);
    }

    private List<SelectListItem> CheckInStatuses()
        => EnumSelectList<AssetStatus>().Where(s => s.Value != AssetStatus.Assigned.ToString()).ToList();

    private async Task PopulateFormListsAsync(AssetDto asset)
    {
        ViewBag.Categories = await OptionsAsync("categories", asset.CategoryId.ToString());
        ViewBag.Locations = await OptionsAsync("locations", asset.LocationId?.ToString());
        ViewBag.Vendors = await OptionsAsync("vendors", asset.VendorId?.ToString());
        ViewBag.Employees = await OptionsAsync("employees", asset.CurrentEmployeeId?.ToString());
    }
}
