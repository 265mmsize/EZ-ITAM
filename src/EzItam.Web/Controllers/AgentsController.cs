using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using EzItam.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Controllers;

public class AgentsController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IAgentService _agents;

    public AgentsController(AppDbContext db, IAgentService agents)
    {
        _db = db;
        _agents = agents;
    }

    public async Task<IActionResult> Index(AgentStatus? status, string? search, CancellationToken ct)
    {
        var q = _db.Agents.AsNoTracking().Include(a => a.Asset).AsQueryable();
        if (status.HasValue) q = q.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(a => a.HostName.Contains(s) || (a.SerialNumber != null && a.SerialNumber.Contains(s)) || (a.LoggedOnUser != null && a.LoggedOnUser.Contains(s)));
        }
        ViewBag.Status = status;
        ViewBag.Search = search;
        ViewBag.Statuses = EnumSelectList(status);
        return View(await q.OrderByDescending(a => a.LastSeenAt).Take(1000).ToListAsync(ct));
    }

    public async Task<IActionResult> Details(int id, string? q, CancellationToken ct)
    {
        var agent = await _db.Agents.AsNoTracking().Include(a => a.Asset).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (agent is null) return NotFound();
        var software = _db.AgentSoftware.AsNoTracking().Where(s => s.AgentId == id);
        if (!string.IsNullOrWhiteSpace(q)) software = software.Where(s => s.Name.Contains(q) || (s.Publisher != null && s.Publisher.Contains(q)));
        ViewBag.Software = await software.OrderBy(s => s.Name).Take(2000).ToListAsync(ct);
        ViewBag.SoftwareCount = await _db.AgentSoftware.CountAsync(s => s.AgentId == id, ct);
        ViewBag.Query = q;
        ViewBag.Assets = await AssetListAsync(_db, agent.AssetId);
        return View(agent);
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            await _agents.ApproveAsync(id);
            Success("에이전트를 승인했습니다. 다음 접속부터 인벤토리를 전송합니다.");
        }
        catch (DomainException ex) { Error(ex.Message); }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(int id)
    {
        try
        {
            await _agents.DisableAsync(id);
            Success("에이전트를 사용 중지했습니다. 토큰이 폐기되었습니다.");
        }
        catch (DomainException ex) { Error(ex.Message); }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Link(int id, int? assetId)
    {
        try
        {
            await _agents.LinkAssetAsync(id, assetId);
            Success(assetId is null ? "자산 연결을 해제했습니다." : "자산과 연결했습니다.");
        }
        catch (DomainException ex) { Error(ex.Message); }
        return RedirectToAction(nameof(Details), new { id });
    }
}
