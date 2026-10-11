using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using EzItam.Web.Services;
using EzItam.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class LicensesController : BaseController
{
    private readonly AppDbContext _db;
    private readonly ILicenseService _licenses;

    public LicensesController(AppDbContext db, ILicenseService licenses)
    {
        _db = db;
        _licenses = licenses;
    }

    public async Task<IActionResult> Index(string? search, bool expiringOnly, CancellationToken ct)
    {
        ViewBag.Search = search;
        ViewBag.ExpiringOnly = expiringOnly;
        return View(await _licenses.ListAsync(search, expiringOnly, ct));
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var license = await _licenses.GetAsync(id, ct);
        if (license is null) return NotFound();
        return View(license);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Create()
    {
        await PopulateAsync(null);
        return View(new SoftwareLicense { TotalSeats = 1 });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SoftwareLicense license, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            _db.SoftwareLicenses.Add(license);
            await _db.SaveChangesAsync(ct);
            Success("라이선스가 등록되었습니다.");
            return RedirectToAction(nameof(Details), new { id = license.Id });
        }
        await PopulateAsync(license);
        return View(license);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var license = await _db.SoftwareLicenses.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, ct);
        if (license is null) return NotFound();
        await PopulateAsync(license);
        return View(license);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SoftwareLicense input, CancellationToken ct)
    {
        if (id != input.Id) return BadRequest();
        var license = await _db.SoftwareLicenses.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (license is null) return NotFound();
        if (ModelState.IsValid)
        {
            license.Name = input.Name;
            license.Publisher = input.Publisher;
            license.Version = input.Version;
            license.LicenseType = input.LicenseType;
            license.LicenseKey = input.LicenseKey;
            license.TotalSeats = input.TotalSeats;
            license.PurchaseDate = input.PurchaseDate;
            license.ExpiryDate = input.ExpiryDate;
            license.VendorId = input.VendorId;
            license.Cost = input.Cost;
            license.Notes = input.Notes;
            await _db.SaveChangesAsync(ct);
            Success("라이선스 정보가 저장되었습니다.");
            return RedirectToAction(nameof(Details), new { id });
        }
        await PopulateAsync(input);
        return View(input);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var license = await _db.SoftwareLicenses.Include(l => l.Assignments).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (license is null) return NotFound();
        if (license.UsedSeats > 0)
        {
            Error("할당된 사용자/장비가 있는 라이선스는 삭제할 수 없습니다. 먼저 할당을 해제하세요.");
            return RedirectToAction(nameof(Details), new { id });
        }
        _db.SoftwareLicenses.Remove(license);
        await _db.SaveChangesAsync(ct);
        Success("라이선스가 삭제되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Assign(int id, CancellationToken ct)
    {
        var license = await _db.SoftwareLicenses.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, ct);
        if (license is null) return NotFound();
        ViewBag.Employees = await EmployeeListAsync(_db);
        ViewBag.Assets = await AssetListAsync(_db);
        return View(new LicenseAssignViewModel { LicenseId = license.Id, LicenseName = license.Name });
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(LicenseAssignViewModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _licenses.AssignAsync(model.LicenseId, model.EmployeeId, model.AssetId, model.Notes, ct);
                Success("라이선스가 할당되었습니다.");
                return RedirectToAction(nameof(Details), new { id = model.LicenseId });
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        ViewBag.Employees = await EmployeeListAsync(_db, model.EmployeeId);
        ViewBag.Assets = await AssetListAsync(_db, model.AssetId);
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Release(int id, int licenseId, CancellationToken ct)
    {
        try
        {
            await _licenses.ReleaseAsync(id, ct);
            Success("라이선스 할당이 해제되었습니다.");
        }
        catch (DomainException ex)
        {
            Error(ex.Message);
        }
        return RedirectToAction(nameof(Details), new { id = licenseId });
    }

    private async Task PopulateAsync(SoftwareLicense? license)
    {
        ViewBag.Vendors = await VendorListAsync(_db, license?.VendorId);
        ViewBag.Types = EnumSelectList(license?.LicenseType);
    }
}
