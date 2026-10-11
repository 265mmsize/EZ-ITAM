namespace EzItam.Api.Controllers;

[Authorize]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboard;
    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
    {
        var s = await _dashboard.GetSummaryAsync(ct);
        return Ok(new DashboardDto
        {
            TotalAssets = s.TotalAssets, AssignedAssets = s.AssignedAssets, InStockAssets = s.InStockAssets,
            InRepairAssets = s.InRepairAssets, RetiredAssets = s.RetiredAssets, TotalPurchaseValue = s.TotalPurchaseValue,
            WarrantyExpiringCount = s.WarrantyExpiringCount, WarrantyExpiredCount = s.WarrantyExpiredCount,
            TotalLicenses = s.TotalLicenses, LicensesExpiringCount = s.LicensesExpiringCount,
            LicensesOverAllocated = s.LicensesOverAllocated, OpenMaintenanceCount = s.OpenMaintenanceCount,
            ByCategory = s.ByCategory, ByStatus = s.ByStatus, ByDepartment = s.ByDepartment,
            WarrantyExpiring = s.WarrantyExpiring.Select(a => Map.ToDto(a)).ToList(),
            LicensesExpiring = s.LicensesExpiring.Select(l => Map.ToDto(l)).ToList(),
            RecentAssignments = s.RecentAssignments.Select(Map.ToDto).ToList(),
            OpenMaintenance = s.OpenMaintenance.Select(Map.ToDto).ToList()
        });
    }
}
