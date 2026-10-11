namespace EzItam.Api.Controllers;

[Authorize]
public class AuditController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public AuditController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PageResult<AuditDto>>> List([FromQuery] string? entity, [FromQuery] string? user,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? key, [FromQuery] int page = 1, CancellationToken ct = default)
    {
        var q = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(entity)) q = q.Where(l => l.EntityName == entity);
        if (!string.IsNullOrWhiteSpace(key)) q = q.Where(l => l.EntityKey == key);
        if (!string.IsNullOrWhiteSpace(user)) q = q.Where(l => l.UserName != null && l.UserName.Contains(user));
        if (from.HasValue) q = q.Where(l => l.Timestamp >= from.Value.Date);
        if (to.HasValue) q = q.Where(l => l.Timestamp < to.Value.Date.AddDays(1));
        q = q.OrderByDescending(l => l.Timestamp);
        var result = await PagedResult<AuditLog>.CreateAsync(q, page, 50, ct);
        return Ok(new PageResult<AuditDto>
        {
            Items = result.Items.Select(Map.ToDto).ToList(), Page = result.Page, PageSize = result.PageSize, TotalCount = result.TotalCount
        });
    }

    [HttpGet("entities")]
    public async Task<ActionResult<List<string>>> Entities(CancellationToken ct)
        => Ok(await _db.AuditLogs.Select(l => l.EntityName).Distinct().OrderBy(x => x).ToListAsync(ct));
}
