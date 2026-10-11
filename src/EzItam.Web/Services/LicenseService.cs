using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Services;

public interface ILicenseService
{
    Task<List<SoftwareLicense>> ListAsync(string? search, bool expiringOnly, CancellationToken ct = default);
    Task<SoftwareLicense?> GetAsync(int id, CancellationToken ct = default);
    Task<LicenseAssignment> AssignAsync(int licenseId, int? employeeId, int? assetId, string? notes, CancellationToken ct = default);
    Task ReleaseAsync(int assignmentId, CancellationToken ct = default);
}

public class LicenseService : ILicenseService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public LicenseService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Task<List<SoftwareLicense>> ListAsync(string? search, bool expiringOnly, CancellationToken ct = default)
    {
        var q = _db.SoftwareLicenses
            .Include(l => l.Vendor)
            .Include(l => l.Assignments)
            .AsNoTracking()
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(l => l.Name.Contains(s) || (l.Publisher != null && l.Publisher.Contains(s)));
        }
        if (expiringOnly)
        {
            var limit = DateTime.Today.AddDays(30);
            q = q.Where(l => l.ExpiryDate != null && l.ExpiryDate <= limit);
        }
        return q.OrderBy(l => l.Name).ToListAsync(ct);
    }

    public Task<SoftwareLicense?> GetAsync(int id, CancellationToken ct = default)
        => _db.SoftwareLicenses
            .Include(l => l.Vendor)
            .Include(l => l.Assignments.OrderByDescending(a => a.AssignedAt)).ThenInclude(a => a.Employee)
            .Include(l => l.Assignments).ThenInclude(a => a.Asset)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<LicenseAssignment> AssignAsync(int licenseId, int? employeeId, int? assetId, string? notes, CancellationToken ct = default)
    {
        if (employeeId is null && assetId is null)
            throw new DomainException("사용자 또는 장비 중 하나는 반드시 지정해야 합니다.");

        var license = await _db.SoftwareLicenses
            .Include(l => l.Assignments)
            .FirstOrDefaultAsync(l => l.Id == licenseId, ct)
            ?? throw new DomainException("라이선스를 찾을 수 없습니다.");

        if (license.IsExpired)
            throw new DomainException("만료된 라이선스는 할당할 수 없습니다.");
        if (license.AvailableSeats <= 0)
            throw new DomainException($"잔여 수량이 없습니다. (보유 {license.TotalSeats} / 사용 {license.UsedSeats})");

        var duplicate = license.Assignments.Any(a => a.ReleasedAt == null
            && ((employeeId.HasValue && a.EmployeeId == employeeId) || (assetId.HasValue && a.AssetId == assetId)));
        if (duplicate)
            throw new DomainException("이미 동일 대상에 할당된 라이선스입니다.");

        if (employeeId.HasValue && !await _db.Employees.AnyAsync(e => e.Id == employeeId && e.IsActive, ct))
            throw new DomainException("재직 중인 사용자를 찾을 수 없습니다.");
        if (assetId.HasValue && !await _db.Assets.AnyAsync(a => a.Id == assetId, ct))
            throw new DomainException("장비를 찾을 수 없습니다.");

        var assignment = new LicenseAssignment
        {
            SoftwareLicenseId = licenseId,
            EmployeeId = employeeId,
            AssetId = assetId,
            AssignedAt = DateTime.Now,
            AssignedBy = _currentUser.UserName,
            Notes = notes
        };
        _db.LicenseAssignments.Add(assignment);
        await _db.SaveChangesAsync(ct);
        return assignment;
    }

    public async Task ReleaseAsync(int assignmentId, CancellationToken ct = default)
    {
        var assignment = await _db.LicenseAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId, ct)
            ?? throw new DomainException("할당 정보를 찾을 수 없습니다.");
        if (assignment.ReleasedAt is not null)
            throw new DomainException("이미 해제된 할당입니다.");
        assignment.ReleasedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
    }
}
