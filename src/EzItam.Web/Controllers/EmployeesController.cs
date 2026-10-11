using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class EmployeesController : BaseController
{
    private readonly AppDbContext _db;
    public EmployeesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, int? departmentId, bool includeInactive, CancellationToken ct)
    {
        var q = _db.Employees.AsNoTracking().Include(e => e.Department).Include(e => e.Assets).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(e => e.Name.Contains(s) || e.EmployeeNo.Contains(s) || (e.Email != null && e.Email.Contains(s)));
        }
        if (departmentId.HasValue) q = q.Where(e => e.DepartmentId == departmentId);
        if (!includeInactive) q = q.Where(e => e.IsActive);
        ViewBag.Search = search;
        ViewBag.DepartmentId = departmentId;
        ViewBag.IncludeInactive = includeInactive;
        ViewBag.Departments = await DepartmentListAsync(_db, departmentId);
        return View(await q.OrderBy(e => e.Department!.Name).ThenBy(e => e.Name).Take(1000).ToListAsync(ct));
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var emp = await _db.Employees.AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Assets).ThenInclude(a => a.Category)
            .Include(e => e.Assignments.OrderByDescending(a => a.AssignedAt)).ThenInclude(a => a.Asset)
            .Include(e => e.LicenseAssignments.Where(l => l.ReleasedAt == null)).ThenInclude(l => l.SoftwareLicense)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        return emp is null ? NotFound() : View(emp);
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = await DepartmentListAsync(_db);
        return View("Form", new Employee());
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee model, CancellationToken ct)
    {
        model.EmployeeNo = model.EmployeeNo.Trim();
        if (await _db.Employees.AnyAsync(e => e.EmployeeNo == model.EmployeeNo, ct))
            ModelState.AddModelError(nameof(Employee.EmployeeNo), "이미 등록된 사번입니다.");
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await DepartmentListAsync(_db, model.DepartmentId);
            return View("Form", model);
        }
        _db.Employees.Add(model);
        await _db.SaveChangesAsync(ct);
        Success("사원이 등록되었습니다.");
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _db.Employees.FindAsync(new object[] { id }, ct);
        if (item is null) return NotFound();
        ViewBag.Departments = await DepartmentListAsync(_db, item.DepartmentId);
        return View("Form", item);
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();
        model.EmployeeNo = model.EmployeeNo.Trim();
        if (await _db.Employees.AnyAsync(e => e.EmployeeNo == model.EmployeeNo && e.Id != id, ct))
            ModelState.AddModelError(nameof(Employee.EmployeeNo), "이미 등록된 사번입니다.");
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await DepartmentListAsync(_db, model.DepartmentId);
            return View("Form", model);
        }
        var item = await _db.Employees.FindAsync(new object[] { id }, ct);
        if (item is null) return NotFound();
        item.EmployeeNo = model.EmployeeNo; item.Name = model.Name; item.Email = model.Email; item.Title = model.Title;
        item.DepartmentId = model.DepartmentId; item.IsActive = model.IsActive;
        await _db.SaveChangesAsync(ct);
        Success("사원 정보가 저장되었습니다.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.Employees.Include(e => e.Assets).Include(e => e.Assignments).FirstOrDefaultAsync(e => e.Id == id, ct);
        if (item is null) return NotFound();
        if (item.Assets.Count > 0) { Error("사용중인 자산이 있는 사원은 삭제할 수 없습니다. 먼저 반납 처리하세요."); return RedirectToAction(nameof(Details), new { id }); }
        if (item.Assignments.Count > 0) { Error("자산 불출 이력이 있는 사원은 삭제할 수 없습니다. 퇴직 처리로 대신하세요."); return RedirectToAction(nameof(Details), new { id }); }
        _db.Employees.Remove(item);
        await _db.SaveChangesAsync(ct);
        Success("사원이 삭제되었습니다.");
        return RedirectToAction(nameof(Index));
    }
}
