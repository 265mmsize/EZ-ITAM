namespace EzItam.Api.Controllers;

[Authorize]
public class CategoriesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> List(CancellationToken ct)
        => Ok((await _db.AssetCategories.AsNoTracking().Include(c => c.Assets).OrderBy(c => c.Code).ToListAsync(ct)).Select(Map.ToDto).ToList());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Get(int id, CancellationToken ct)
    {
        var c = await _db.AssetCategories.AsNoTracking().Include(x => x.Assets).FirstOrDefaultAsync(x => x.Id == id, ct);
        return c is null ? NotFoundError("분류") : Ok(Map.ToDto(c));
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryDto dto, CancellationToken ct)
    {
        var model = Inbound.ToCategory(dto);
        if (await _db.AssetCategories.AnyAsync(c => c.Code == model.Code, ct)) throw new DomainException("이미 사용 중인 코드입니다.");
        _db.AssetCategories.Add(model);
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(model));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CategoryDto dto, CancellationToken ct)
    {
        var input = Inbound.ToCategory(dto);
        if (await _db.AssetCategories.AnyAsync(c => c.Code == input.Code && c.Id != id, ct)) throw new DomainException("이미 사용 중인 코드입니다.");
        var item = await _db.AssetCategories.FindAsync(new object[] { id }, ct) ?? throw new DomainException("분류를 찾을 수 없습니다.");
        item.Code = input.Code; item.Name = input.Name; item.Description = input.Description; item.DepreciationMonths = input.DepreciationMonths;
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(item));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.AssetCategories.Include(c => c.Assets).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (item is null) return NotFoundError("분류");
        if (item.Assets.Count > 0) throw new DomainException("해당 분류에 속한 자산이 있어 삭제할 수 없습니다.");
        _db.AssetCategories.Remove(item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
