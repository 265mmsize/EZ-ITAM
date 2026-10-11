using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class DepartmentsController : BaseController
{
    private readonly AppDbContext _db;
    public DepartmentsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _db.Departments.AsNoTracking().Include(d => d.Employees).OrderBy(d => d.Code).ToListAsync(ct));

    [Authorize(Policy = "CanEdit")]
    public IActionResult Create() => View("Form", new Department());

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department model, CancellationToken ct)
    {
        model.Code = model.Code.Trim().ToUpperInvariant();
        if (await _db.Departments.AnyAsync(d => d.Code == model.Code, ct))
            ModelState.AddModelError(nameof(Department.Code), "이미 사용 중인 부서코드입니다.");
        if (!ModelState.IsValid) return View("Form", model);
        _db.Departments.Add(model);
        await _db.SaveChangesAsync(ct);
        Success("부서가 등록되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _db.Departments.FindAsync(new object[] { id }, ct);
        return item is null ? NotFound() : View("Form", item);
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();
        model.Code = model.Code.Trim().ToUpperInvariant();
        if (await _db.Departments.AnyAsync(d => d.Code == model.Code && d.Id != id, ct))
            ModelState.AddModelError(nameof(Department.Code), "이미 사용 중인 부서코드입니다.");
        if (!ModelState.IsValid) return View("Form", model);
        var item = await _db.Departments.FindAsync(new object[] { id }, ct);
        if (item is null) return NotFound();
        item.Code = model.Code; item.Name = model.Name; item.IsActive = model.IsActive;
        await _db.SaveChangesAsync(ct);
        Success("부서가 저장되었습니다.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == id, ct);
        if (item is null) return NotFound();
        if (item.Employees.Count > 0) { Error("소속 사원이 있는 부서는 삭제할 수 없습니다. 사용 해제로 처리하세요."); return RedirectToAction(nameof(Index)); }
        _db.Departments.Remove(item);
        await _db.SaveChangesAsync(ct);
        Success("부서가 삭제되었습니다.");
        return RedirectToAction(nameof(Index));
    }
}
