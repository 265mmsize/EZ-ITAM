namespace EzItam.Api.Controllers;

[Authorize]
public class MaintenanceController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public MaintenanceController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<MaintenanceDto>>> List([FromQuery] bool openOnly, [FromQuery] MaintenanceType? type, CancellationToken ct)
    {
        var q = _db.MaintenanceRecords.AsNoTracking().Include(m => m.Asset).Include(m => m.Vendor).AsQueryable();
        if (openOnly) q = q.Where(m => m.CompletedAt == null);
        if (type.HasValue) q = q.Where(m => m.Type == type);
        var rows = await q.OrderByDescending(m => m.Date).ThenByDescending(m => m.Id).Take(500).ToListAsync(ct);
        return Ok(rows.Select(Map.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaintenanceDto>> Get(int id, CancellationToken ct)
    {
        var m = await _db.MaintenanceRecords.AsNoTracking().Include(x => x.Asset).Include(x => x.Vendor).FirstOrDefaultAsync(x => x.Id == id, ct);
        return m is null ? NotFoundError("유지보수 이력") : Ok(Map.ToDto(m));
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<MaintenanceDto>> Create([FromBody] MaintenanceDto dto, [FromQuery] bool setInRepair, CancellationToken ct)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == dto.AssetId, ct) ?? throw new DomainException("자산을 찾을 수 없습니다.");
        var record = Inbound.ToMaintenance(dto);
        record.CreatedBy = CurrentUser;
        _db.MaintenanceRecords.Add(record);
        if (setInRepair && record.CompletedAt is null && asset.Status != AssetStatus.Assigned)
            asset.Status = AssetStatus.InRepair;
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(await _db.MaintenanceRecords.Include(x => x.Asset).Include(x => x.Vendor).FirstAsync(x => x.Id == record.Id, ct)));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<MaintenanceDto>> Update(int id, [FromBody] MaintenanceDto dto, [FromQuery] bool restoreToStock, CancellationToken ct)
    {
        var record = await _db.MaintenanceRecords.Include(m => m.Asset).FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new DomainException("유지보수 이력을 찾을 수 없습니다.");
        record.Type = dto.Type;
        record.Date = dto.Date;
        record.Description = dto.Description ?? string.Empty;
        record.Cost = dto.Cost;
        record.VendorId = dto.VendorId;
        record.CompletedAt = dto.CompletedAt;
        if (restoreToStock && record.CompletedAt.HasValue && record.Asset!.Status == AssetStatus.InRepair)
            record.Asset.Status = AssetStatus.InStock;
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(await _db.MaintenanceRecords.Include(x => x.Asset).Include(x => x.Vendor).FirstAsync(x => x.Id == id, ct)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var record = await _db.MaintenanceRecords.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (record is null) return NotFoundError("유지보수 이력");
        _db.MaintenanceRecords.Remove(record);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
