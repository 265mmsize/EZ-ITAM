using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class LocationsController : BaseController
{
    private readonly AppDbContext _db;
    public LocationsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _db.Locations.AsNoTracking().Include(l => l.Assets).OrderBy(l => l.Building).ThenBy(l => l.Floor).ThenBy(l => l.Name).ToListAsync(ct));

    [Authorize(Policy = "CanEdit")]
    public IActionResult Create() => View("Form", new Location());

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Location model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Form", model);
        _db.Locations.Add(model);
        await _db.SaveChangesAsync(ct);
        Success("위치가 등록되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _db.Locations.FindAsync(new object[] { id }, ct);
        return item is null ? NotFound() : View("Form", item);
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Location model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", model);
        var item = await _db.Locations.FindAsync(new object[] { id }, ct);
        if (item is null) return NotFound();
        item.Name = model.Name; item.Building = model.Building; item.Floor = model.Floor; item.Room = model.Room; item.IsActive = model.IsActive;
        await _db.SaveChangesAsync(ct);
        Success("위치가 저장되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.Locations.Include(l => l.Assets).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (item is null) return NotFound();
        if (item.Assets.Count > 0) { Error("해당 위치에 자산이 있어 삭제할 수 없습니다. 사용 해제로 처리하세요."); return RedirectToAction(nameof(Index)); }
        _db.Locations.Remove(item);
        await _db.SaveChangesAsync(ct);
        Success("위치가 삭제되었습니다.");
        return RedirectToAction(nameof(Index));
    }
}
