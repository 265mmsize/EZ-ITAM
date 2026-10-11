using EzItam.Web.Api;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

[Authorize]
public class AgentsController : BaseController
{
    public AgentsController(ItamApiClient api) : base(api) { }

    public async Task<IActionResult> Index(AgentStatus? status, string? search)
    {
        ViewBag.Status = status;
        ViewBag.Search = search;
        ViewBag.Statuses = EnumSelectList(status);
        return View(await Api.ListAgentsAsync(status, search));
    }

    public async Task<IActionResult> Details(int id, string? q)
    {
        var agent = await Api.GetAgentAsync(id);
        var software = string.IsNullOrWhiteSpace(q)
            ? agent.Software
            : agent.Software.Where(s => s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || (s.Publisher?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        ViewBag.Software = software.OrderBy(s => s.Name).ToList();
        ViewBag.SoftwareCount = agent.SoftwareCount;
        ViewBag.Query = q;
        ViewBag.Assets = await OptionsAsync("assets", agent.AssetId?.ToString());
        return View(agent);
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try { await Api.ApproveAgentAsync(id); Success("에이전트를 승인했습니다. 다음 접속부터 인벤토리를 전송합니다."); }
        catch (ApiException ex) { Error(ex.Message); }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(int id)
    {
        try { await Api.DisableAgentAsync(id); Success("에이전트를 사용 중지했습니다. 토큰이 폐기되었습니다."); }
        catch (ApiException ex) { Error(ex.Message); }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Link(int id, int? assetId)
    {
        try { await Api.LinkAgentAsync(id, assetId); Success(assetId is null ? "자산 연결을 해제했습니다." : "자산과 연결했습니다."); }
        catch (ApiException ex) { Error(ex.Message); }
        return RedirectToAction(nameof(Details), new { id });
    }
}
