namespace EzItam.Api.Controllers;

public class AssetsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAssetService _assets;

    public AssetsController(AppDbContext db, IAssetService assets)
    {
        _db = db;
        _assets = assets;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<AssetDto>>> List([FromQuery] AssetFilter filter, CancellationToken ct)
    {
        var page = await _assets.SearchAsync(filter, ct);
        return Ok(new PageResult<AssetDto>
        {
            Items = page.Items.Select(a => Map.ToDto(a)).ToList(),
            Page = page.Page, PageSize = page.PageSize, TotalCount = page.TotalCount
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssetDto>> Get(int id, CancellationToken ct)
    {
        var asset = await _assets.GetAsync(id, ct);
        return asset is null ? NotFoundError("자산") : Ok(Map.ToDto(asset, detail: true));
    }

    [HttpGet("next-tag")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<NextTagResponse>> NextTag([FromQuery] int categoryId, CancellationToken ct)
        => Ok(new NextTagResponse { Tag = await _assets.GenerateAssetTagAsync(categoryId, ct) });

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] AssetFilter filter, CancellationToken ct)
    {
        var rows = await _assets.Query(filter).Include(a => a.Vendor).ToListAsync(ct);
        var bytes = CsvExporter.Build(rows,
            ("자산번호", a => a.AssetTag), ("자산명", a => a.Name), ("분류", a => a.Category?.Name),
            ("상태", a => a.Status), ("제조사", a => a.Manufacturer), ("모델명", a => a.Model),
            ("시리얼번호", a => a.SerialNumber), ("사용자", a => a.CurrentEmployee?.Name),
            ("사번", a => a.CurrentEmployee?.EmployeeNo), ("부서", a => a.CurrentEmployee?.Department?.Name),
            ("위치", a => a.Location?.Name), ("공급업체", a => a.Vendor?.Name),
            ("구매일", a => a.PurchaseDate), ("구매가격", a => a.PurchasePrice), ("보증만료일", a => a.WarrantyExpiry),
            ("호스트명", a => a.HostName), ("IP", a => a.IpAddress), ("MAC", a => a.MacAddress),
            ("OS", a => a.OperatingSystem), ("비고", a => a.Notes));
        return File(bytes, "text/csv; charset=utf-8", $"assets_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    [HttpPost]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<AssetDto>> Create([FromBody] AssetDto dto, CancellationToken ct)
    {
        if (dto.CategoryId == 0) throw new DomainException("분류를 선택하세요.");
        var asset = await _assets.CreateAsync(Inbound.ToAsset(dto), ct);
        var saved = await _assets.GetAsync(asset.Id, ct);
        return Ok(Map.ToDto(saved!, detail: true));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<AssetDto>> Update(int id, [FromBody] AssetDto dto, CancellationToken ct)
    {
        var input = Inbound.ToAsset(dto);
        input.Id = id;
        await _assets.UpdateAsync(input, ct);
        var saved = await _assets.GetAsync(id, ct);
        return saved is null ? NotFoundError("자산") : Ok(Map.ToDto(saved, detail: true));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _assets.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/checkout")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<AssignmentDto>> CheckOut(int id, [FromBody] CheckOutRequest req, CancellationToken ct)
    {
        var assignment = await _assets.CheckOutAsync(id, req.EmployeeId, req.Notes, ct);
        return Ok(Map.ToDto(assignment));
    }

    [HttpPost("{id:int}/checkin")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<AssignmentDto>> CheckIn(int id, [FromBody] CheckInRequest req, CancellationToken ct)
    {
        var assignment = await _assets.CheckInAsync(id, req.NewStatus, req.LocationId, req.Notes, ct);
        return Ok(Map.ToDto(assignment));
    }

    [HttpPost("{id:int}/status")]
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest req, CancellationToken ct)
    {
        await _assets.ChangeStatusAsync(id, req.NewStatus, req.Notes, ct);
        return NoContent();
    }

    [HttpGet("labels")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> GetLabels([FromQuery] string ids, CancellationToken ct)
    {
        try
        {
            byte[] base64Decoded = Convert.FromBase64String(ids);
            var idList = System.Text.Encoding.UTF8.GetString(base64Decoded).Split(',').Select(int.Parse).ToList();
            var res = await _assets.GetListAssetLabelsByIdsAsync(idList, ct);
            return Ok(res);

        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
