namespace EzItam.Api.Controllers;

[Authorize]
public class EmployeesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public EmployeesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> List([FromQuery] string? search, [FromQuery] int? departmentId, [FromQuery] bool includeInactive, CancellationToken ct)
    {
        var q = _db.Employees.AsNoTracking().Include(e => e.Department).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(e => e.Name.Contains(s) || e.EmployeeNo.Contains(s) || (e.Email != null && e.Email.Contains(s)));
        }
        if (departmentId.HasValue) q = q.Where(e => e.DepartmentId == departmentId);
        if (!includeInactive) q = q.Where(e => e.IsActive);
        var rows = await q.OrderBy(e => e.Department!.Name).ThenBy(e => e.Name).Take(1000).ToListAsync(ct);
        return Ok(rows.Select(e => Map.ToDto(e)).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> Get(int id, CancellationToken ct)
    {
        var e = await _db.Employees.AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Assets).ThenInclude(a => a.Category)
            .Include(x => x.Assignments).ThenInclude(a => a.Asset)
            .Include(x => x.LicenseAssignments.Where(l => l.ReleasedAt == null)).ThenInclude(l => l.SoftwareLicense)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? NotFoundError("사원") : Ok(Map.ToDto(e, detail: true));
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeDto dto, CancellationToken ct)
    {
        var model = Inbound.ToEmployee(dto);
        if (await _db.Employees.AnyAsync(e => e.EmployeeNo == model.EmployeeNo, ct)) throw new DomainException("이미 등록된 사번입니다.");
        _db.Employees.Add(model);
        await _db.SaveChangesAsync(ct);
        return Ok((await Get(model.Id, ct)).Value!);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<EmployeeDto>> Update(int id, [FromBody] EmployeeDto dto, CancellationToken ct)
    {
        var input = Inbound.ToEmployee(dto);
        if (await _db.Employees.AnyAsync(e => e.EmployeeNo == input.EmployeeNo && e.Id != id, ct)) throw new DomainException("이미 등록된 사번입니다.");
        var item = await _db.Employees.FindAsync(new object[] { id }, ct) ?? throw new DomainException("사원을 찾을 수 없습니다.");
        item.EmployeeNo = input.EmployeeNo; item.Name = input.Name; item.Email = input.Email; item.Title = input.Title;
        item.DepartmentId = input.DepartmentId; item.IsActive = input.IsActive;
        await _db.SaveChangesAsync(ct);
        return Ok((await Get(id, ct)).Value!);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.Employees.Include(e => e.Assets).Include(e => e.Assignments).FirstOrDefaultAsync(e => e.Id == id, ct);
        if (item is null) return NotFoundError("사원");
        if (item.Assets.Count > 0) throw new DomainException("사용중인 자산이 있는 사원은 삭제할 수 없습니다. 먼저 반납 처리하세요.");
        if (item.Assignments.Count > 0) throw new DomainException("자산 불출 이력이 있는 사원은 삭제할 수 없습니다. 퇴직 처리로 대신하세요.");
        _db.Employees.Remove(item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
