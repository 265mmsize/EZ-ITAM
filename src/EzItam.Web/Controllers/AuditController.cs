using EzItam.Web.Data;
using EzItam.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class AuditController : BaseController
{
    private readonly AppDbContext _db;
    public AuditController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? entity, string? user, DateTime? from, DateTime? to, int page = 1, CancellationToken ct = default)
    {
        var q = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(entity)) q = q.Where(l => l.EntityName == entity);
        if (!string.IsNullOrWhiteSpace(user)) q = q.Where(l => l.UserName != null && l.UserName.Contains(user));
        if (from.HasValue) q = q.Where(l => l.Timestamp >= from.Value.Date);
        if (to.HasValue) q = q.Where(l => l.Timestamp < to.Value.Date.AddDays(1));
        q = q.OrderByDescending(l => l.Timestamp);

        ViewBag.Entity = entity;
        ViewBag.User = user;
        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.Entities = await _db.AuditLogs.Select(l => l.EntityName).Distinct().OrderBy(x => x).ToListAsync(ct);
        return View(await PagedResult<Models.Entities.AuditLog>.CreateAsync(q, page, 50, ct));
    }
}
