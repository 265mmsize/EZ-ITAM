namespace EzItam.Api.Controllers;

[Authorize]
public class LocationsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public LocationsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> List(CancellationToken ct)
        => Ok((await _db.Locations.AsNoTracking().Include(l => l.Assets).OrderBy(l => l.Building).ThenBy(l => l.Floor).ThenBy(l => l.Name).ToListAsync(ct)).Select(Map.ToDto).ToList());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> Get(int id, CancellationToken ct)
    {
        var l = await _db.Locations.AsNoTracking().Include(x => x.Assets).FirstOrDefaultAsync(x => x.Id == id, ct);
        return l is null ? NotFoundError("위치") : Ok(Map.ToDto(l));
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<LocationDto>> Create([FromBody] LocationDto dto, CancellationToken ct)
    {
        var model = Inbound.ToLocation(dto);
        _db.Locations.Add(model);
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(model));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<LocationDto>> Update(int id, [FromBody] LocationDto dto, CancellationToken ct)
    {
        var item = await _db.Locations.FindAsync(new object[] { id }, ct) ?? throw new DomainException("위치를 찾을 수 없습니다.");
        var input = Inbound.ToLocation(dto);
        item.Name = input.Name; item.Building = input.Building; item.Floor = input.Floor; item.Room = input.Room; item.IsActive = input.IsActive;
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(item));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.Locations.Include(l => l.Assets).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (item is null) return NotFoundError("위치");
        if (item.Assets.Count > 0) throw new DomainException("해당 위치에 자산이 있어 삭제할 수 없습니다. 사용 해제로 처리하세요.");
        _db.Locations.Remove(item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
