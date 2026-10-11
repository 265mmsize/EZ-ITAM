using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class VendorsController : BaseController
{
    private readonly AppDbContext _db;
    public VendorsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _db.Vendors.AsNoTracking().Include(v => v.Assets).Include(v => v.Licenses).OrderBy(v => v.Name).ToListAsync(ct));

    [Authorize(Policy = "CanEdit")]
    public IActionResult Create() => View("Form", new Vendor());

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Vendor model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Form", model);
        _db.Vendors.Add(model);
        await _db.SaveChangesAsync(ct);
        Success("공급업체가 등록되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _db.Vendors.FindAsync(new object[] { id }, ct);
        return item is null ? NotFound() : View("Form", item);
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Vendor model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", model);
        var item = await _db.Vendors.FindAsync(new object[] { id }, ct);
        if (item is null) return NotFound();
        item.Name = model.Name; item.ContactName = model.ContactName; item.Phone = model.Phone; item.Email = model.Email; item.Notes = model.Notes; item.ErpVendorId = model.ErpVendorId;
        await _db.SaveChangesAsync(ct);
        Success("공급업체가 저장되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.Vendors.FindAsync(new object[] { id }, ct);
        if (item is null) return NotFound();
        _db.Vendors.Remove(item);
        await _db.SaveChangesAsync(ct);
        Success("공급업체가 삭제되었습니다. (연결된 자산의 공급업체는 비워집니다.)");
        return RedirectToAction(nameof(Index));
    }
}
