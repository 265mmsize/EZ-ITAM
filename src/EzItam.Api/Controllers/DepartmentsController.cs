namespace EzItam.Api.Controllers;

[Authorize]
public class DepartmentsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public DepartmentsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> List(CancellationToken ct)
        => Ok((await _db.Departments.AsNoTracking().Include(d => d.Employees).OrderBy(d => d.Code).ToListAsync(ct)).Select(Map.ToDto).ToList());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> Get(int id, CancellationToken ct)
    {
        var d = await _db.Departments.AsNoTracking().Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == id, ct);
        return d is null ? NotFoundError("부서") : Ok(Map.ToDto(d));
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] DepartmentDto dto, CancellationToken ct)
    {
        var model = Inbound.ToDepartment(dto);
        if (await _db.Departments.AnyAsync(d => d.Code == model.Code, ct)) throw new DomainException("이미 사용 중인 부서코드입니다.");
        _db.Departments.Add(model);
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(model));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<DepartmentDto>> Update(int id, [FromBody] DepartmentDto dto, CancellationToken ct)
    {
        var input = Inbound.ToDepartment(dto);
        if (await _db.Departments.AnyAsync(d => d.Code == input.Code && d.Id != id, ct)) throw new DomainException("이미 사용 중인 부서코드입니다.");
        var item = await _db.Departments.FindAsync(new object[] { id }, ct) ?? throw new DomainException("부서를 찾을 수 없습니다.");
        item.Code = input.Code; item.Name = input.Name; item.IsActive = input.IsActive;
        await _db.SaveChangesAsync(ct);
        return Ok(Map.ToDto(item));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == id, ct);
        if (item is null) return NotFoundError("부서");
        if (item.Employees.Count > 0) throw new DomainException("소속 사원이 있는 부서는 삭제할 수 없습니다. 사용 해제로 처리하세요.");
        _db.Departments.Remove(item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
