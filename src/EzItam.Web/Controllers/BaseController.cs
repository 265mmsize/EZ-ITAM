using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using EzItam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public abstract class BaseController : Controller
{
    protected void Success(string message) => TempData["Alert.Success"] = message;
    protected void Error(string message) => TempData["Alert.Error"] = message;

    protected static List<SelectListItem> EnumSelectList<TEnum>(TEnum? selected = null) where TEnum : struct, Enum
        => EnumDisplay.All<TEnum>()
            .Select(x => new SelectListItem(x.Name, x.Value.ToString(), selected.HasValue && selected.Value.Equals(x.Value)))
            .ToList();

    protected static async Task<List<SelectListItem>> CategoryListAsync(AppDbContext db, int? selected = null)
        => await db.AssetCategories.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == selected)).ToListAsync();

    protected static async Task<List<SelectListItem>> LocationListAsync(AppDbContext db, int? selected = null)
        => await db.Locations.AsNoTracking().Where(l => l.IsActive || l.Id == selected).OrderBy(l => l.Name)
            .Select(l => new SelectListItem(l.Name, l.Id.ToString(), l.Id == selected)).ToListAsync();

    protected static async Task<List<SelectListItem>> VendorListAsync(AppDbContext db, int? selected = null)
        => await db.Vendors.AsNoTracking().OrderBy(v => v.Name)
            .Select(v => new SelectListItem(v.Name, v.Id.ToString(), v.Id == selected)).ToListAsync();

    protected static async Task<List<SelectListItem>> DepartmentListAsync(AppDbContext db, int? selected = null)
        => await db.Departments.AsNoTracking().Where(d => d.IsActive || d.Id == selected).OrderBy(d => d.Name)
            .Select(d => new SelectListItem(d.Name, d.Id.ToString(), d.Id == selected)).ToListAsync();

    protected static async Task<List<SelectListItem>> EmployeeListAsync(AppDbContext db, int? selected = null, bool activeOnly = true)
        => await db.Employees.AsNoTracking().Include(e => e.Department)
            .Where(e => !activeOnly || e.IsActive || e.Id == selected)
            .OrderBy(e => e.Department!.Name).ThenBy(e => e.Name)
            .Select(e => new SelectListItem(
                (e.Department != null ? e.Department.Name + " / " : "") + e.Name + " (" + e.EmployeeNo + ")",
                e.Id.ToString(), e.Id == selected))
            .ToListAsync();

    protected static async Task<List<SelectListItem>> AssetListAsync(AppDbContext db, int? selected = null)
        => await db.Assets.AsNoTracking()
            .Where(a => a.Status != AssetStatus.Disposed && a.Status != AssetStatus.Lost || a.Id == selected)
            .OrderBy(a => a.AssetTag)
            .Select(a => new SelectListItem(a.AssetTag + " " + a.Name, a.Id.ToString(), a.Id == selected))
            .ToListAsync();
}
