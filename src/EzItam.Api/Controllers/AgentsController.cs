namespace EzItam.Api.Controllers;

/// <summary>관리자용 에이전트 조회·승인·연결 API. 에이전트 기기 자체의 호출은 AgentApiController 가 담당합니다.</summary>
[Authorize]
public class AgentsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAgentService _agents;

    public AgentsController(AppDbContext db, IAgentService agents)
    {
        _db = db;
        _agents = agents;
    }

    [HttpGet]
    public async Task<ActionResult<List<AgentDto>>> List([FromQuery] AgentStatus? status, [FromQuery] string? search, CancellationToken ct)
    {
        var q = _db.Agents.AsNoTracking().Include(a => a.Asset).AsQueryable();
        if (status.HasValue) q = q.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(a => a.HostName.Contains(s) || (a.SerialNumber != null && a.SerialNumber.Contains(s)) || (a.LoggedOnUser != null && a.LoggedOnUser.Contains(s)));
        }
        var rows = await q.OrderByDescending(a => a.LastSeenAt).Take(1000).ToListAsync(ct);
        return Ok(rows.Select(a => Map.ToDto(a)).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AgentDto>> Get(int id, CancellationToken ct)
    {
        var a = await _db.Agents.AsNoTracking().Include(x => x.Asset).Include(x => x.Software).FirstOrDefaultAsync(x => x.Id == id, ct);
        return a is null ? NotFoundError("에이전트") : Ok(Map.ToDto(a, detail: true));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        await _agents.ApproveAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Disable(int id, CancellationToken ct)
    {
        await _agents.DisableAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/link")]
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Link(int id, [FromBody] AgentLinkRequest req, CancellationToken ct)
    {
        await _agents.LinkAssetAsync(id, req.AssetId, ct);
        return NoContent();
    }
}
