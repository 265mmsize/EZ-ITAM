using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Services;

public class AssetFilter
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public AssetStatus? Status { get; set; }
    public int? LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public int? EmployeeId { get; set; }
    public bool WarrantyExpiringOnly { get; set; }
    public string Sort { get; set; } = "tag";
    public bool Desc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public interface IAssetService
{
    IQueryable<Asset> Query(AssetFilter filter);
    Task<PagedResult<Asset>> SearchAsync(AssetFilter filter, CancellationToken ct = default);
    Task<Asset?> GetAsync(int id, CancellationToken ct = default);
    Task<Asset> CreateAsync(Asset asset, CancellationToken ct = default);
    Task UpdateAsync(Asset asset, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<AssetAssignment> CheckOutAsync(int assetId, int employeeId, string? notes, CancellationToken ct = default);
    Task<AssetAssignment> CheckInAsync(int assetId, AssetStatus newStatus, int? locationId, string? notes, CancellationToken ct = default);
    Task ChangeStatusAsync(int assetId, AssetStatus newStatus, string? notes, CancellationToken ct = default);
    Task<string> GenerateAssetTagAsync(int categoryId, CancellationToken ct = default);
}

public class AssetService : IAssetService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AssetService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public IQueryable<Asset> Query(AssetFilter f)
    {
        var q = _db.Assets
            .Include(a => a.Category)
            .Include(a => a.Location)
            .Include(a => a.CurrentEmployee).ThenInclude(e => e!.Department)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var s = f.Search.Trim();
            q = q.Where(a => a.AssetTag.Contains(s)
                || a.Name.Contains(s)
                || (a.SerialNumber != null && a.SerialNumber.Contains(s))
                || (a.Model != null && a.Model.Contains(s))
                || (a.HostName != null && a.HostName.Contains(s))
                || (a.IpAddress != null && a.IpAddress.Contains(s))
                || (a.CurrentEmployee != null && a.CurrentEmployee.Name.Contains(s)));
        }
        if (f.CategoryId.HasValue) q = q.Where(a => a.CategoryId == f.CategoryId);
        if (f.Status.HasValue) q = q.Where(a => a.Status == f.Status);
        if (f.LocationId.HasValue) q = q.Where(a => a.LocationId == f.LocationId);
        if (f.EmployeeId.HasValue) q = q.Where(a => a.CurrentEmployeeId == f.EmployeeId);
        if (f.DepartmentId.HasValue) q = q.Where(a => a.CurrentEmployee != null && a.CurrentEmployee.DepartmentId == f.DepartmentId);
        if (f.WarrantyExpiringOnly)
        {
            var limit = DateTime.Today.AddDays(30);
            q = q.Where(a => a.WarrantyExpiry != null && a.WarrantyExpiry <= limit);
        }

        q = (f.Sort, f.Desc) switch
        {
            ("name", false) => q.OrderBy(a => a.Name),
            ("name", true) => q.OrderByDescending(a => a.Name),
            ("status", false) => q.OrderBy(a => a.Status).ThenBy(a => a.AssetTag),
            ("status", true) => q.OrderByDescending(a => a.Status).ThenBy(a => a.AssetTag),
            ("category", false) => q.OrderBy(a => a.Category!.Name).ThenBy(a => a.AssetTag),
            ("category", true) => q.OrderByDescending(a => a.Category!.Name).ThenBy(a => a.AssetTag),
            ("purchase", false) => q.OrderBy(a => a.PurchaseDate),
            ("purchase", true) => q.OrderByDescending(a => a.PurchaseDate),
            ("warranty", false) => q.OrderBy(a => a.WarrantyExpiry),
            ("warranty", true) => q.OrderByDescending(a => a.WarrantyExpiry),
            ("updated", false) => q.OrderBy(a => a.UpdatedAt),
            ("updated", true) => q.OrderByDescending(a => a.UpdatedAt),
            (_, true) => q.OrderByDescending(a => a.AssetTag),
            _ => q.OrderBy(a => a.AssetTag)
        };
        return q;
    }

    public Task<PagedResult<Asset>> SearchAsync(AssetFilter filter, CancellationToken ct = default)
        => PagedResult<Asset>.CreateAsync(Query(filter), filter.Page, filter.PageSize, ct);

    public Task<Asset?> GetAsync(int id, CancellationToken ct = default)
        => _db.Assets
            .Include(a => a.Category)
            .Include(a => a.Vendor)
            .Include(a => a.Location)
            .Include(a => a.CurrentEmployee).ThenInclude(e => e!.Department)
            .Include(a => a.Assignments.OrderByDescending(x => x.AssignedAt)).ThenInclude(x => x.Employee)
            .Include(a => a.MaintenanceRecords.OrderByDescending(m => m.Date)).ThenInclude(m => m.Vendor)
            .Include(a => a.LicenseAssignments.Where(l => l.ReleasedAt == null)).ThenInclude(l => l.SoftwareLicense)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Asset> CreateAsync(Asset asset, CancellationToken ct = default)
    {
        asset.AssetTag = asset.AssetTag.Trim().ToUpperInvariant();
        if (await _db.Assets.AnyAsync(a => a.AssetTag == asset.AssetTag, ct))
            throw new DomainException($"자산번호 '{asset.AssetTag}'는 이미 사용 중입니다.");

        if (asset.CurrentEmployeeId.HasValue)
        {
            asset.Status = AssetStatus.Assigned;
        }
        else if (asset.Status == AssetStatus.Assigned)
        {
            throw new DomainException("사용자를 지정하지 않으면 '사용중' 상태로 등록할 수 없습니다.");
        }

        _db.Assets.Add(asset);
        await _db.SaveChangesAsync(ct);

        if (asset.CurrentEmployeeId.HasValue)
        {
            _db.AssetAssignments.Add(new AssetAssignment
            {
                AssetId = asset.Id,
                EmployeeId = asset.CurrentEmployeeId.Value,
                AssignedAt = DateTime.Now,
                AssignedBy = _currentUser.UserName,
                Notes = "자산 등록 시 지정"
            });
            await _db.SaveChangesAsync(ct);
        }
        return asset;
    }

    public async Task UpdateAsync(Asset input, CancellationToken ct = default)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == input.Id, ct)
            ?? throw new DomainException("자산을 찾을 수 없습니다.");

        input.AssetTag = input.AssetTag.Trim().ToUpperInvariant();
        if (await _db.Assets.AnyAsync(a => a.AssetTag == input.AssetTag && a.Id != input.Id, ct))
            throw new DomainException($"자산번호 '{input.AssetTag}'는 이미 사용 중입니다.");

        // 사용자/상태는 불출·반납·상태변경 기능으로만 변경 (이력 보존)
        asset.AssetTag = input.AssetTag;
        asset.Name = input.Name;
        asset.CategoryId = input.CategoryId;
        asset.Manufacturer = input.Manufacturer;
        asset.Model = input.Model;
        asset.SerialNumber = input.SerialNumber;
        asset.VendorId = input.VendorId;
        asset.PurchaseDate = input.PurchaseDate;
        asset.PurchasePrice = input.PurchasePrice;
        asset.WarrantyExpiry = input.WarrantyExpiry;
        asset.LocationId = input.LocationId;
        asset.IpAddress = input.IpAddress;
        asset.MacAddress = input.MacAddress;
        asset.HostName = input.HostName;
        asset.OperatingSystem = input.OperatingSystem;
        asset.ErpAssetNumber = input.ErpAssetNumber;
        asset.Notes = input.Notes;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new DomainException("자산을 찾을 수 없습니다.");
        if (asset.Status == AssetStatus.Assigned)
            throw new DomainException("사용중인 자산은 삭제할 수 없습니다. 먼저 반납 처리하세요.");
        _db.Assets.Remove(asset);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AssetAssignment> CheckOutAsync(int assetId, int employeeId, string? notes, CancellationToken ct = default)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == assetId, ct)
            ?? throw new DomainException("자산을 찾을 수 없습니다.");
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, ct)
            ?? throw new DomainException("사용자를 찾을 수 없습니다.");

        if (!employee.IsActive) throw new DomainException("퇴직 처리된 사용자에게는 불출할 수 없습니다.");
        if (asset.Status == AssetStatus.Assigned)
            throw new DomainException("이미 사용중인 자산입니다. 먼저 반납 처리하세요.");
        if (asset.Status is AssetStatus.Disposed or AssetStatus.Lost)
            throw new DomainException("폐기 또는 분실된 자산은 불출할 수 없습니다.");

        var assignment = new AssetAssignment
        {
            AssetId = asset.Id,
            EmployeeId = employee.Id,
            AssignedAt = DateTime.Now,
            AssignedBy = _currentUser.UserName,
            Notes = notes
        };
        asset.CurrentEmployeeId = employee.Id;
        asset.Status = AssetStatus.Assigned;
        _db.AssetAssignments.Add(assignment);
        await _db.SaveChangesAsync(ct);
        return assignment;
    }

    public async Task<AssetAssignment> CheckInAsync(int assetId, AssetStatus newStatus, int? locationId, string? notes, CancellationToken ct = default)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == assetId, ct)
            ?? throw new DomainException("자산을 찾을 수 없습니다.");
        if (asset.Status != AssetStatus.Assigned || asset.CurrentEmployeeId is null)
            throw new DomainException("사용중 상태가 아닌 자산은 반납할 수 없습니다.");
        if (newStatus == AssetStatus.Assigned)
            throw new DomainException("반납 후 상태는 '사용중'이 될 수 없습니다.");

        var open = await _db.AssetAssignments
            .Where(x => x.AssetId == assetId && x.ReturnedAt == null)
            .OrderByDescending(x => x.AssignedAt)
            .FirstOrDefaultAsync(ct);

        if (open is null)
        {
            open = new AssetAssignment
            {
                AssetId = asset.Id,
                EmployeeId = asset.CurrentEmployeeId.Value,
                AssignedAt = asset.UpdatedAt,
                AssignedBy = "system",
                Notes = "이력 누락 보정"
            };
            _db.AssetAssignments.Add(open);
        }

        open.ReturnedAt = DateTime.Now;
        open.ReturnedBy = _currentUser.UserName;
        if (!string.IsNullOrWhiteSpace(notes))
            open.Notes = string.IsNullOrWhiteSpace(open.Notes) ? notes : $"{open.Notes} / 반납: {notes}";

        asset.CurrentEmployeeId = null;
        asset.Status = newStatus;
        if (locationId.HasValue) asset.LocationId = locationId;

        await _db.SaveChangesAsync(ct);
        return open;
    }

    public async Task ChangeStatusAsync(int assetId, AssetStatus newStatus, string? notes, CancellationToken ct = default)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == assetId, ct)
            ?? throw new DomainException("자산을 찾을 수 없습니다.");
        if (newStatus == AssetStatus.Assigned)
            throw new DomainException("'사용중' 상태는 불출 기능을 통해서만 설정할 수 있습니다.");
        if (asset.Status == AssetStatus.Assigned)
            throw new DomainException("사용중인 자산은 먼저 반납 처리해야 상태를 변경할 수 있습니다.");
        if (asset.Status == newStatus) return;

        asset.Status = newStatus;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            var stamp = $"[{DateTime.Now:yyyy-MM-dd} {_currentUser.UserName}] 상태변경({newStatus}): {notes}";
            asset.Notes = string.IsNullOrWhiteSpace(asset.Notes) ? stamp : $"{asset.Notes}\n{stamp}";
        }
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>분류코드-연도-일련번호 형식의 자산번호를 생성합니다. 예: NB-2025-0003</summary>
    public async Task<string> GenerateAssetTagAsync(int categoryId, CancellationToken ct = default)
    {
        var category = await _db.AssetCategories.FirstOrDefaultAsync(c => c.Id == categoryId, ct)
            ?? throw new DomainException("분류를 찾을 수 없습니다.");
        var prefix = $"{category.Code.ToUpperInvariant()}-{DateTime.Today:yyyy}-";
        var existing = await _db.Assets
            .Where(a => a.AssetTag.StartsWith(prefix))
            .Select(a => a.AssetTag)
            .ToListAsync(ct);

        var max = existing
            .Select(t => int.TryParse(t.Substring(prefix.Length), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"{prefix}{max + 1:0000}";
    }
}
