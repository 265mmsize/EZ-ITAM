namespace EzItam.Api.Controllers;

/// <summary>화면의 드롭다운 목록을 한 번에 가져오는 조회 전용 API입니다.</summary>
[Authorize]
public class OptionsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public OptionsController(AppDbContext db) => _db = db;

    [HttpGet("categories")]
    public async Task<ActionResult<List<SelectOption>>> Categories(CancellationToken ct)
        => Ok(await _db.AssetCategories.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new SelectOption { Value = c.Id.ToString(), Text = c.Name }).ToListAsync(ct));

    [HttpGet("locations")]
    public async Task<ActionResult<List<SelectOption>>> Locations(CancellationToken ct)
        => Ok(await _db.Locations.AsNoTracking().Where(l => l.IsActive).OrderBy(l => l.Name)
            .Select(l => new SelectOption { Value = l.Id.ToString(), Text = l.Name }).ToListAsync(ct));

    [HttpGet("vendors")]
    public async Task<ActionResult<List<SelectOption>>> Vendors(CancellationToken ct)
        => Ok(await _db.Vendors.AsNoTracking().OrderBy(v => v.Name)
            .Select(v => new SelectOption { Value = v.Id.ToString(), Text = v.Name }).ToListAsync(ct));

    [HttpGet("departments")]
    public async Task<ActionResult<List<SelectOption>>> Departments(CancellationToken ct)
        => Ok(await _db.Departments.AsNoTracking().Where(d => d.IsActive).OrderBy(d => d.Name)
            .Select(d => new SelectOption { Value = d.Id.ToString(), Text = d.Name }).ToListAsync(ct));

    [HttpGet("employees")]
    public async Task<ActionResult<List<SelectOption>>> Employees(CancellationToken ct)
    {
        var rows = await _db.Employees.AsNoTracking().Include(e => e.Department).Where(e => e.IsActive)
            .OrderBy(e => e.Department!.Name).ThenBy(e => e.Name).ToListAsync(ct);
        return Ok(rows.Select(e => new SelectOption
        {
            Value = e.Id.ToString(),
            Text = (e.Department != null ? e.Department.Name + " / " : "") + e.Name + " (" + e.EmployeeNo + ")"
        }).ToList());
    }

    [HttpGet("assets")]
    public async Task<ActionResult<List<SelectOption>>> Assets(CancellationToken ct)
        => Ok(await _db.Assets.AsNoTracking()
            .Where(a => a.Status != AssetStatus.Disposed && a.Status != AssetStatus.Lost)
            .OrderBy(a => a.AssetTag)
            .Select(a => new SelectOption { Value = a.Id.ToString(), Text = a.AssetTag + " " + a.Name }).ToListAsync(ct));
}
