using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using EzItam.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class MaintenanceController : BaseController
{
    private readonly AppDbContext _db;

    public MaintenanceController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(bool openOnly, MaintenanceType? type, CancellationToken ct)
    {
        var q = _db.MaintenanceRecords.AsNoTracking()
            .Include(m => m.Asset).Include(m => m.Vendor)
            .AsQueryable();
        if (openOnly) q = q.Where(m => m.CompletedAt == null);
        if (type.HasValue) q = q.Where(m => m.Type == type);
        ViewBag.OpenOnly = openOnly;
        ViewBag.Type = type;
        ViewBag.Types = EnumSelectList<MaintenanceType>(type);
        return View(await q.OrderByDescending(m => m.Date).ThenByDescending(m => m.Id).Take(500).ToListAsync(ct));
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Create(int assetId, CancellationToken ct)
    {
        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == assetId, ct);
        if (asset is null) return NotFound();
        ViewBag.Asset = asset;
        ViewBag.Vendors = await VendorListAsync(_db);
        ViewBag.Types = EnumSelectList<MaintenanceType>();
        return View(new MaintenanceRecord { AssetId = assetId, Date = DateTime.Today });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceRecord record, bool setInRepair, CancellationToken ct)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == record.AssetId, ct);
        if (asset is null) return NotFound();
        if (ModelState.IsValid)
        {
            _db.MaintenanceRecords.Add(record);
            if (setInRepair && record.CompletedAt is null && asset.Status != AssetStatus.Assigned)
                asset.Status = AssetStatus.InRepair;
            await _db.SaveChangesAsync(ct);
            Success("유지보수 이력이 등록되었습니다.");
            return RedirectToAction("Details", "Assets", new { id = record.AssetId });
        }
        ViewBag.Asset = asset;
        ViewBag.Vendors = await VendorListAsync(_db, record.VendorId);
        ViewBag.Types = EnumSelectList<MaintenanceType>(record.Type);
        return View(record);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var record = await _db.MaintenanceRecords.AsNoTracking().Include(m => m.Asset).FirstOrDefaultAsync(m => m.Id == id, ct);
        if (record is null) return NotFound();
        ViewBag.Asset = record.Asset;
        ViewBag.Vendors = await VendorListAsync(_db, record.VendorId);
        ViewBag.Types = EnumSelectList<MaintenanceType>(record.Type);
        return View(record);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MaintenanceRecord input, bool restoreToStock, CancellationToken ct)
    {
        if (id != input.Id) return BadRequest();
        var record = await _db.MaintenanceRecords.Include(m => m.Asset).FirstOrDefaultAsync(m => m.Id == id, ct);
        if (record is null) return NotFound();
        if (ModelState.IsValid)
        {
            record.Type = input.Type;
            record.Date = input.Date;
            record.Description = input.Description;
            record.Cost = input.Cost;
            record.VendorId = input.VendorId;
            record.CompletedAt = input.CompletedAt;
            if (restoreToStock && record.CompletedAt.HasValue && record.Asset!.Status == AssetStatus.InRepair)
                record.Asset.Status = AssetStatus.InStock;
            await _db.SaveChangesAsync(ct);
            Success("유지보수 이력이 저장되었습니다.");
            return RedirectToAction("Details", "Assets", new { id = record.AssetId });
        }
        ViewBag.Asset = record.Asset;
        ViewBag.Vendors = await VendorListAsync(_db, input.VendorId);
        ViewBag.Types = EnumSelectList<MaintenanceType>(input.Type);
        input.Asset = record.Asset;
        return View(input);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var record = await _db.MaintenanceRecords.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (record is null) return NotFound();
        var assetId = record.AssetId;
        _db.MaintenanceRecords.Remove(record);
        await _db.SaveChangesAsync(ct);
        Success("유지보수 이력이 삭제되었습니다.");
        return RedirectToAction("Details", "Assets", new { id = assetId });
    }
}
