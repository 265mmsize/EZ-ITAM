using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using EzItam.Web.Services;
using EzItam.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class AssetsController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IAssetService _assets;

    public AssetsController(AppDbContext db, IAssetService assets)
    {
        _db = db;
        _assets = assets;
    }

    public async Task<IActionResult> Index(AssetFilter filter, CancellationToken ct)
    {
        var result = await _assets.SearchAsync(filter, ct);
        ViewBag.Categories = await CategoryListAsync(_db, filter.CategoryId);
        ViewBag.Locations = await LocationListAsync(_db, filter.LocationId);
        ViewBag.Departments = await DepartmentListAsync(_db, filter.DepartmentId);
        ViewBag.Statuses = EnumSelectList(filter.Status);
        return View(new AssetIndexViewModel { Filter = filter, Result = result });
    }

    public async Task<IActionResult> Export(AssetFilter filter, CancellationToken ct)
    {
        var rows = await _assets.Query(filter).Include(a => a.Vendor).ToListAsync(ct);
        var bytes = CsvExporter.Build(rows,
            ("자산번호", a => a.AssetTag),
            ("자산명", a => a.Name),
            ("분류", a => a.Category?.Name),
            ("상태", a => a.Status),
            ("제조사", a => a.Manufacturer),
            ("모델명", a => a.Model),
            ("시리얼번호", a => a.SerialNumber),
            ("사용자", a => a.CurrentEmployee?.Name),
            ("사번", a => a.CurrentEmployee?.EmployeeNo),
            ("부서", a => a.CurrentEmployee?.Department?.Name),
            ("위치", a => a.Location?.Name),
            ("공급업체", a => a.Vendor?.Name),
            ("구매일", a => a.PurchaseDate),
            ("구매가격", a => a.PurchasePrice),
            ("보증만료일", a => a.WarrantyExpiry),
            ("호스트명", a => a.HostName),
            ("IP", a => a.IpAddress),
            ("MAC", a => a.MacAddress),
            ("OS", a => a.OperatingSystem),
            ("비고", a => a.Notes));
        return File(bytes, "text/csv; charset=utf-8", $"assets_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var asset = await _assets.GetAsync(id, ct);
        if (asset is null) return NotFound();
        ViewBag.AuditLogs = await _db.AuditLogs.AsNoTracking()
            .Where(l => l.EntityName == nameof(Asset) && l.EntityKey == id.ToString())
            .OrderByDescending(l => l.Timestamp).Take(20).ToListAsync(ct);
        return View(asset);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Create(int? categoryId)
    {
        var asset = new Asset { Status = AssetStatus.InStock, CategoryId = categoryId ?? 0 };
        if (categoryId.HasValue)
        {
            try { asset.AssetTag = await _assets.GenerateAssetTagAsync(categoryId.Value); }
            catch (DomainException) { }
        }
        await PopulateFormListsAsync(asset);
        return View(asset);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Asset asset, CancellationToken ct)
    {
        if (asset.CategoryId == 0) ModelState.AddModelError(nameof(Asset.CategoryId), "분류를 선택하세요.");
        if (ModelState.IsValid)
        {
            try
            {
                await _assets.CreateAsync(asset, ct);
                Success($"자산 {asset.AssetTag} 이(가) 등록되었습니다.");
                return RedirectToAction(nameof(Details), new { id = asset.Id });
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        await PopulateFormListsAsync(asset);
        return View(asset);
    }

    [HttpGet]
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> NextTag(int categoryId)
    {
        try { return Json(new { tag = await _assets.GenerateAssetTagAsync(categoryId) }); }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        if (asset is null) return NotFound();
        await PopulateFormListsAsync(asset);
        return View(asset);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Asset asset, CancellationToken ct)
    {
        if (id != asset.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await _assets.UpdateAsync(asset, ct);
                Success("자산 정보가 저장되었습니다.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        await PopulateFormListsAsync(asset);
        return View(asset);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            await _assets.DeleteAsync(id, ct);
            Success("자산이 삭제되었습니다.");
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            Error(ex.Message);
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ---------- 불출 ----------
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> CheckOut(int id, CancellationToken ct)
    {
        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        if (asset is null) return NotFound();
        ViewBag.Employees = await EmployeeListAsync(_db);
        return View(new CheckOutViewModel { AssetId = asset.Id, AssetTag = asset.AssetTag, AssetName = asset.Name });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(CheckOutViewModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _assets.CheckOutAsync(model.AssetId, model.EmployeeId!.Value, model.Notes, ct);
                Success("불출 처리되었습니다.");
                return RedirectToAction(nameof(Details), new { id = model.AssetId });
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        ViewBag.Employees = await EmployeeListAsync(_db, model.EmployeeId);
        return View(model);
    }

    // ---------- 반납 ----------
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> CheckIn(int id, CancellationToken ct)
    {
        var asset = await _db.Assets.AsNoTracking().Include(a => a.CurrentEmployee).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (asset is null) return NotFound();
        ViewBag.Locations = await LocationListAsync(_db, asset.LocationId);
        ViewBag.Statuses = EnumSelectList<AssetStatus>().Where(s => s.Value != AssetStatus.Assigned.ToString()).ToList();
        return View(new CheckInViewModel
        {
            AssetId = asset.Id, AssetTag = asset.AssetTag, AssetName = asset.Name,
            CurrentEmployeeName = asset.CurrentEmployee?.DisplayName, LocationId = asset.LocationId
        });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInViewModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _assets.CheckInAsync(model.AssetId, model.NewStatus, model.LocationId, model.Notes, ct);
                Success("반납 처리되었습니다.");
                return RedirectToAction(nameof(Details), new { id = model.AssetId });
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        ViewBag.Locations = await LocationListAsync(_db, model.LocationId);
        ViewBag.Statuses = EnumSelectList<AssetStatus>().Where(s => s.Value != AssetStatus.Assigned.ToString()).ToList();
        return View(model);
    }

    // ---------- 상태 변경 ----------
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> ChangeStatus(int id, CancellationToken ct)
    {
        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        if (asset is null) return NotFound();
        ViewBag.Statuses = EnumSelectList<AssetStatus>().Where(s => s.Value != AssetStatus.Assigned.ToString()).ToList();
        return View(new ChangeStatusViewModel { AssetId = asset.Id, AssetTag = asset.AssetTag, AssetName = asset.Name, CurrentStatus = asset.Status, NewStatus = asset.Status });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(ChangeStatusViewModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _assets.ChangeStatusAsync(model.AssetId, model.NewStatus, model.Notes, ct);
                Success("상태가 변경되었습니다.");
                return RedirectToAction(nameof(Details), new { id = model.AssetId });
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        ViewBag.Statuses = EnumSelectList<AssetStatus>().Where(s => s.Value != AssetStatus.Assigned.ToString()).ToList();
        return View(model);
    }

    private async Task PopulateFormListsAsync(Asset asset)
    {
        ViewBag.Categories = await CategoryListAsync(_db, asset.CategoryId);
        ViewBag.Locations = await LocationListAsync(_db, asset.LocationId);
        ViewBag.Vendors = await VendorListAsync(_db, asset.VendorId);
        ViewBag.Employees = await EmployeeListAsync(_db, asset.CurrentEmployeeId);
    }
}
