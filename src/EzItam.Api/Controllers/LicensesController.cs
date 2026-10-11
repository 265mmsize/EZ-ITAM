namespace EzItam.Api.Controllers;

[Authorize]
public class LicensesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILicenseService _licenses;

    public LicensesController(AppDbContext db, ILicenseService licenses)
    {
        _db = db;
        _licenses = licenses;
    }

    [HttpGet]
    public async Task<ActionResult<List<SoftwareLicenseDto>>> List([FromQuery] string? search, [FromQuery] bool expiringOnly, CancellationToken ct)
    {
        var rows = await _licenses.ListAsync(search, expiringOnly, ct);
        return Ok(rows.Select(l => Map.ToDto(l)).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SoftwareLicenseDto>> Get(int id, CancellationToken ct)
    {
        var license = await _licenses.GetAsync(id, ct);
        return license is null ? NotFoundError("라이선스") : Ok(Map.ToDto(license, detail: true));
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<SoftwareLicenseDto>> Create([FromBody] SoftwareLicenseDto dto, CancellationToken ct)
    {
        var license = Inbound.ToLicense(dto);
        _db.SoftwareLicenses.Add(license);
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto((await _licenses.GetAsync(license.Id, ct))!, detail: true));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<SoftwareLicenseDto>> Update(int id, [FromBody] SoftwareLicenseDto dto, CancellationToken ct)
    {
        var license = await _db.SoftwareLicenses.FirstOrDefaultAsync(l => l.Id == id, ct) ?? throw new DomainException("라이선스를 찾을 수 없습니다.");
        var input = Inbound.ToLicense(dto);
        license.Name = input.Name; license.Publisher = input.Publisher; license.Version = input.Version;
        license.LicenseType = input.LicenseType; license.LicenseKey = input.LicenseKey; license.TotalSeats = input.TotalSeats;
        license.PurchaseDate = input.PurchaseDate; license.ExpiryDate = input.ExpiryDate; license.VendorId = input.VendorId;
        license.Cost = input.Cost; license.Notes = input.Notes;
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto((await _licenses.GetAsync(id, ct))!, detail: true));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var license = await _db.SoftwareLicenses.Include(l => l.Assignments).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (license is null) return NotFoundError("라이선스");
        if (license.UsedSeats > 0) throw new DomainException("할당된 사용자/장비가 있는 라이선스는 삭제할 수 없습니다. 먼저 할당을 해제하세요.");
        _db.SoftwareLicenses.Remove(license);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:int}/assignments")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<LicenseAssignmentDto>> Assign(int id, [FromBody] LicenseAssignRequest req, CancellationToken ct)
    {
        var a = await _licenses.AssignAsync(id, req.EmployeeId, req.AssetId, req.Notes, ct);
        return Ok(Map.ToDto(a));
    }

    [HttpPost("assignments/{assignmentId:int}/release")]
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> Release(int assignmentId, CancellationToken ct)
    {
        await _licenses.ReleaseAsync(assignmentId, ct);
        return NoContent();
    }
}
