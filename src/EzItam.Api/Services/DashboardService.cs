using EzItam.Api.Data;
using EzItam.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Api.Services;

public class DashboardSummary
{
    public int TotalAssets { get; set; }
    public int AssignedAssets { get; set; }
    public int InStockAssets { get; set; }
    public int InRepairAssets { get; set; }
    public int RetiredAssets { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public int WarrantyExpiringCount { get; set; }
    public int WarrantyExpiredCount { get; set; }
    public int TotalLicenses { get; set; }
    public int LicensesExpiringCount { get; set; }
    public int LicensesOverAllocated { get; set; }
    public int OpenMaintenanceCount { get; set; }
    public List<KeyValuePair<string, int>> ByCategory { get; set; } = new();
    public List<KeyValuePair<AssetStatus, int>> ByStatus { get; set; } = new();
    public List<KeyValuePair<string, int>> ByDepartment { get; set; } = new();
    public List<Asset> WarrantyExpiring { get; set; } = new();
    public List<SoftwareLicense> LicensesExpiring { get; set; } = new();
    public List<AssetAssignment> RecentAssignments { get; set; } = new();
    public List<MaintenanceRecord> OpenMaintenance { get; set; } = new();
}

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync(CancellationToken ct = default);
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var soon = today.AddDays(30);
        var assets = _db.Assets.AsNoTracking();

        var summary = new DashboardSummary
        {
            TotalAssets = await assets.CountAsync(ct),
            AssignedAssets = await assets.CountAsync(a => a.Status == AssetStatus.Assigned, ct),
            InStockAssets = await assets.CountAsync(a => a.Status == AssetStatus.InStock || a.Status == AssetStatus.Idle, ct),
            InRepairAssets = await assets.CountAsync(a => a.Status == AssetStatus.InRepair, ct),
            RetiredAssets = await assets.CountAsync(a => a.Status == AssetStatus.Retired || a.Status == AssetStatus.Disposed || a.Status == AssetStatus.Lost, ct),
            // SQLite 는 decimal SUM 을 지원하지 않으므로 클라이언트에서 합산 (SQL Server 에서도 동일 동작)
            TotalPurchaseValue = (await assets.Where(a => a.Status != AssetStatus.Disposed && a.PurchasePrice != null)
                .Select(a => a.PurchasePrice!.Value).ToListAsync(ct)).Sum(),
            WarrantyExpiringCount = await assets.CountAsync(a => a.WarrantyExpiry != null && a.WarrantyExpiry >= today && a.WarrantyExpiry <= soon, ct),
            WarrantyExpiredCount = await assets.CountAsync(a => a.WarrantyExpiry != null && a.WarrantyExpiry < today && a.Status != AssetStatus.Disposed && a.Status != AssetStatus.Retired, ct),
            TotalLicenses = await _db.SoftwareLicenses.CountAsync(ct),
            LicensesExpiringCount = await _db.SoftwareLicenses.CountAsync(l => l.ExpiryDate != null && l.ExpiryDate >= today && l.ExpiryDate <= soon, ct),
            OpenMaintenanceCount = await _db.MaintenanceRecords.CountAsync(m => m.CompletedAt == null, ct)
        };

        summary.ByCategory = (await assets
                .GroupBy(a => a.Category!.Name)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToListAsync(ct))
            .OrderByDescending(x => x.Count)
            .Select(x => new KeyValuePair<string, int>(x.Key, x.Count))
            .ToList();

        summary.ByStatus = (await assets
                .GroupBy(a => a.Status)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToListAsync(ct))
            .OrderBy(x => x.Key)
            .Select(x => new KeyValuePair<AssetStatus, int>(x.Key, x.Count))
            .ToList();

        summary.ByDepartment = (await assets
                .Where(a => a.CurrentEmployee != null && a.CurrentEmployee.Department != null)
                .GroupBy(a => a.CurrentEmployee!.Department!.Name)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToListAsync(ct))
            .OrderByDescending(x => x.Count)
            .Select(x => new KeyValuePair<string, int>(x.Key, x.Count))
            .ToList();

        summary.WarrantyExpiring = await assets
            .Include(a => a.Category).Include(a => a.CurrentEmployee)
            .Where(a => a.WarrantyExpiry != null && a.WarrantyExpiry >= today && a.WarrantyExpiry <= soon)
            .OrderBy(a => a.WarrantyExpiry)
            .Take(10)
            .ToListAsync(ct);

        var licenses = await _db.SoftwareLicenses.AsNoTracking()
            .Include(l => l.Assignments)
            .ToListAsync(ct);
        summary.LicensesOverAllocated = licenses.Count(l => l.LicenseType is not (LicenseType.Site or LicenseType.Free) && l.UsedSeats > l.TotalSeats);
        summary.LicensesExpiring = licenses
            .Where(l => l.ExpiryDate != null && l.ExpiryDate >= today && l.ExpiryDate <= soon)
            .OrderBy(l => l.ExpiryDate)
            .Take(10)
            .ToList();

        summary.RecentAssignments = await _db.AssetAssignments.AsNoTracking()
            .Include(a => a.Asset).Include(a => a.Employee)
            .OrderByDescending(a => a.ReturnedAt ?? a.AssignedAt)
            .Take(10)
            .ToListAsync(ct);

        summary.OpenMaintenance = await _db.MaintenanceRecords.AsNoTracking()
            .Include(m => m.Asset).Include(m => m.Vendor)
            .Where(m => m.CompletedAt == null)
            .OrderBy(m => m.Date)
            .Take(10)
            .ToListAsync(ct);

        return summary;
    }
}
