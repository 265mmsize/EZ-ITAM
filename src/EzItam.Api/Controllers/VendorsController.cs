namespace EzItam.Api.Controllers;

[Authorize]
public class VendorsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public VendorsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<VendorDto>>> List(CancellationToken ct)
        => Ok((await _db.Vendors.AsNoTracking().Include(v => v.Assets).Include(v => v.Licenses).OrderBy(v => v.Name).ToListAsync(ct)).Select(Map.ToDto).ToList());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VendorDto>> Get(int id, CancellationToken ct)
    {
        var v = await _db.Vendors.AsNoTracking().Include(x => x.Assets).Include(x => x.Licenses).FirstOrDefaultAsync(x => x.Id == id, ct);
        return v is null ? NotFoundError("공급업체") : Ok(Map.ToDto(v));
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<VendorDto>> Create([FromBody] VendorDto dto, CancellationToken ct)
    {
        var model = Inbound.ToVendor(dto);
        _db.Vendors.Add(model);
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(model));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<VendorDto>> Update(int id, [FromBody] VendorDto dto, CancellationToken ct)
    {
        var item = await _db.Vendors.FindAsync(new object[] { id }, ct) ?? throw new DomainException("공급업체를 찾을 수 없습니다.");
        var input = Inbound.ToVendor(dto);
        item.Name = input.Name; item.ContactName = input.ContactName; item.Phone = input.Phone; item.Email = input.Email; item.Notes = input.Notes;
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(item));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.Vendors.FindAsync(new object[] { id }, ct);
        if (item is null) return NotFoundError("공급업체");
        _db.Vendors.Remove(item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
