using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class CategoriesController : BaseController
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _db.AssetCategories.AsNoTracking().Include(c => c.Assets).OrderBy(c => c.Code).ToListAsync(ct));

    [Authorize(Policy = "CanEdit")]
    public IActionResult Create() => View("Form", new AssetCategory());

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetCategory model, CancellationToken ct)
    {
        model.Code = model.Code.Trim().ToUpperInvariant();
        if (await _db.AssetCategories.AnyAsync(c => c.Code == model.Code, ct))
            ModelState.AddModelError(nameof(AssetCategory.Code), "이미 사용 중인 코드입니다.");
        if (!ModelState.IsValid) return View("Form", model);
        _db.AssetCategories.Add(model);
        await _db.SaveChangesAsync(ct);
        Success("분류가 등록되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _db.AssetCategories.FindAsync(new object[] { id }, ct);
        return item is null ? NotFound() : View("Form", item);
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AssetCategory model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();
        model.Code = model.Code.Trim().ToUpperInvariant();
        if (await _db.AssetCategories.AnyAsync(c => c.Code == model.Code && c.Id != id, ct))
            ModelState.AddModelError(nameof(AssetCategory.Code), "이미 사용 중인 코드입니다.");
        if (!ModelState.IsValid) return View("Form", model);
        var item = await _db.AssetCategories.FindAsync(new object[] { id }, ct);
        if (item is null) return NotFound();
        item.Code = model.Code; item.Name = model.Name; item.Description = model.Description; item.DepreciationMonths = model.DepreciationMonths;
        await _db.SaveChangesAsync(ct);
        Success("분류가 저장되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.AssetCategories.Include(c => c.Assets).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (item is null) return NotFound();
        if (item.Assets.Count > 0) { Error("해당 분류에 속한 자산이 있어 삭제할 수 없습니다."); return RedirectToAction(nameof(Index)); }
        _db.AssetCategories.Remove(item);
        await _db.SaveChangesAsync(ct);
        Success("분류가 삭제되었습니다.");
        return RedirectToAction(nameof(Index));
    }
}
