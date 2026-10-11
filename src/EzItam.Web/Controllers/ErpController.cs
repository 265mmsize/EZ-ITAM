using EzItam.Web.Services.Erp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzItam.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class ErpController : BaseController
{
    private readonly IErpSyncService _sync;
    public ErpController(IErpSyncService sync) => _sync = sync;

    public IActionResult Index()
    {
        ViewBag.IsAvailable = _sync.IsAvailable;
        return View(_sync.LastResult);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Sync(CancellationToken ct)
    {
        var result = await _sync.SyncAsync(ct);
        if (result.Succeeded)
            Success($"ERP 동기화 완료 - 부서 신규 {result.DepartmentsInserted}/변경 {result.DepartmentsUpdated}, 사원 신규 {result.EmployeesInserted}/변경 {result.EmployeesUpdated}/퇴직 {result.EmployeesDeactivated}");
        else
            Error($"ERP 동기화 실패: {result.Error}");
        return RedirectToAction(nameof(Index));
    }
}
